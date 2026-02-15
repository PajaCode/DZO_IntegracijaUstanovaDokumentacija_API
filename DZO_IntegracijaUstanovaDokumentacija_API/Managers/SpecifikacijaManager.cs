using DZO_IntegracijaUstanovaDokumentacija_API.Api;
using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DomainClasses;
using HR_API.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{

    public class SpecifikacijaManager
    {
        private readonly GlobosSftpService _sftpService;
        private readonly RazmenaDokumentacijeDb_Context _db;
        private readonly Logovi _logger;

        public SpecifikacijaManager(GlobosSftpService sftpService, RazmenaDokumentacijeDb_Context db, Logovi logger)
        {
            _sftpService = sftpService;
            _db = db;
            _logger = logger;
        }

        // IdMetoda = 1
        public OperationSummary UpisiJsone()
        {
            var summary = new OperationSummary { Message = "Upis JSON fajlova završen." };

            var listaFajlova = _sftpService.ListaJsona();
            summary = summary with { Total = listaFajlova.Count };

            var sb = new StringBuilder();

            using (_sftpService.ConnectScope())
            {
                foreach (var file in listaFajlova)
                {
                    try
                    {
                        var baseName = Path.GetFileNameWithoutExtension(file);
                        var existsCount = _db.DZOI_Vizim_Json.Count(x => x.nazivJson.Contains(baseName) && x.StatusId == 2);

                        if (existsCount == 0)
                        {
                            _db.DZOI_Vizim_Json.Add(new DZOI_Vizim_Json
                            {
                                nazivJson = file,
                                StatusId = 1,
                                SistemskiDatum = DateTime.Now
                            });
                            _db.SaveChanges();
                            summary.Succeeded++;
                            continue;
                        }

                        // rename duplikat na SFTP
                        sb.Clear();
                        sb.Append(baseName).Append('_').Append(existsCount).Append(Path.GetExtension(file));

                        var root = _sftpService.VratiPutanju();
                        _sftpService._sftpClient.RenameFile($"{root}/{file}", $"{root}/{sb}");

                        _db.DZOI_Vizim_Json.Add(new DZOI_Vizim_Json
                        {
                            nazivJson = sb.ToString(),
                            StatusId = 1,
                            SistemskiDatum = DateTime.Now
                        });
                        _db.SaveChanges();

                        summary.Succeeded++;
                    }
                    catch (Exception ex)
                    {
                        summary.Failed++;
                        summary.Errors.Add(new ItemError(null, null, file, ex.Message));
                    }
                }
            }

            var status = summary.Failed == 0 ? ApiExecutionStatus.Succeeded : ApiExecutionStatus.CompletedWithErrors;
            return summary with { Status = status };
        }

        // IdMetoda = 2
        public async Task<OperationSummary> ParsirajIInsertujAsync()
        {
            var summary = new OperationSummary { Message = "Parsiranje i upis specifikacije završen." };

            var rezultat = _db.DZOI_Vizim_Json.Where(x => x.StatusId == 1).ToList();
            summary = summary with { Total = rezultat.Count };

            var settings = new JsonSerializerSettings
            {
                Culture = CultureInfo.GetCultureInfo("sr-Latn-RS")
            };

            foreach (var jsonRow in rezultat)
            {
                var idJson = jsonRow.Id;
                var naziv = jsonRow.nazivJson;

                try
                {
                    var sadrzajFajla = _sftpService.UzmiSadrzajFajla(naziv);

                    FileJson? jsonObject;
                    try
                    {
                        jsonObject = JsonConvert.DeserializeObject<FileJson>(sadrzajFajla, settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(idJson, ex.Message + " - parsiranje JSON-a", 0, 2);
                        summary.Failed++;
                        summary.Errors.Add(new ItemError(idJson, null, naziv, "Greška parsiranja JSON-a: " + ex.Message));
                        continue;
                    }

                    if (jsonObject is null)
                    {
                        _logger.LogError(idJson, "Izabrani JSON je prazan.", 0, 2);
                        summary.Failed++;
                        summary.Errors.Add(new ItemError(idJson, null, naziv, "JSON je prazan"));
                        continue;
                    }

                    // insert
                    List<DZOI_InsertSpecifikacijeRacunaFajlovaResult> spResult;
                    try
                    {
                        spResult = await InsertPodatakaIzJsona(jsonObject, jsonRow);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(idJson, ex.Message + " - insert", 0, 2);
                        summary.Failed++;
                        summary.Errors.Add(new ItemError(idJson, null, naziv, "Greška inserta: " + ex.Message));
                        continue;
                    }

                    var first = spResult?.FirstOrDefault();
                    if (first is null || first.Status != 1)
                    {
                        var poruka = first?.Poruka ?? "Neuspeh (bez poruke iz procedure)";
                        _logger.LogError(idJson, poruka, 0, 2);
                        summary.Failed++;
                        summary.Errors.Add(new ItemError(idJson, null, naziv, poruka));
                        continue;
                    }

                    _logger.AzurirajStatusVizimJson(idJson, 2);
                    summary.Succeeded++;
                }
                catch (Exception ex)
                {
                    // neočekivano po jednom JSON-u (i dalje nastavljamo dalje)
                    _logger.LogError(idJson, ex.Message + " - neočekivana greška", 0, 2);
                    summary.Failed++;
                    summary.Errors.Add(new ItemError(idJson, null, naziv, ex.Message));
                }
            }

            var status = summary.Failed == 0 ? ApiExecutionStatus.Succeeded : ApiExecutionStatus.CompletedWithErrors;
            return summary with { Status = status };
        }

        private async Task<List<DZOI_InsertSpecifikacijeRacunaFajlovaResult>> InsertPodatakaIzJsona(FileJson fileJson, DZOI_Vizim_Json json)
        {
            DataTable specifikacija = new();
            DataTable racun = new();
            DataTable stavka = new();
            DataTable fajl = new();
            List<string> listaFajlova = new();
            string opisJson = JsonConvert.SerializeObject(fileJson);

            var zaglavljeTable = new ZaglavljeTable
            {
                IdJson = json.Id,
                FakturaId = fileJson.Zaglavlje.FakturaId,
                FakturaBroj = fileJson.Zaglavlje.FakturaBroj,
                UstanovaIDMG = fileJson.Zaglavlje.UstanovaIDMG,
                Datum = fileJson.Zaglavlje.Datum,
                OpisJson = opisJson
            };
            specifikacija = HR_API.Helpers.FormatTypeHelper.ToDataTableFromObject(zaglavljeTable);

            List<RacunTable> racuni = new();
            List<StavkaTable> stavke = new();
            List<FajloviTable> fajlovi = new();

            foreach (Racun rac in fileJson.Racun)
            {
                if (!string.IsNullOrEmpty(rac.RacunFajl)) listaFajlova.Add(rac.RacunFajl);
                if (!string.IsNullOrEmpty(rac.UputFajl)) listaFajlova.Add(rac.UputFajl);

                racuni.Add(new RacunTable
                {
                    RacunID = rac.RacunID,
                    RacunDatum = rac.RacunDatum,
                    RacunBrojFiskala = rac.RacunBrojFiskala,
                    UputBroj = rac.UputBroj,
                    BrojKartice = rac.PacijentID,
                    UkupanIznos = rac.IZNOSCLAIM,
                    Participacija = rac.Participacija,
                    Popust = rac.Popust,
                    RacunFajl = rac.RacunFajl,
                    UputFajl = rac.UputFajl,
                });

                foreach (Stavka stav in rac.Stavka)
                {
                    if (!string.IsNullOrEmpty(stav.LabNalazFajl)) listaFajlova.Add(stav.LabNalazFajl);
                    if (!string.IsNullOrEmpty(stav.NalazFajl)) listaFajlova.Add(stav.NalazFajl);
                    if (!string.IsNullOrEmpty(stav.Nalazsistematski)) listaFajlova.Add(stav.Nalazsistematski);

                    stavke.Add(new StavkaTable
                    {
                        StavkaID = stav.StavkaID,
                        UslugaDatum = stav.UslugaDatum,
                        Popust = stav.Popust,
                        PunaCena = stav.PunaCena,
                        ZaIsplatu = stav.ZaUplatu,
                        Valuta = stav.Valuta,
                        UslugaID = stav.UslugaID,
                        SARADNIKID = stav.SARADNIKID,
                        SARADNIKNAZIV = stav.SARADNIKNAZIV,
                        LabNalazFajl = stav.LabNalazFajl,
                        NalazFajl = stav.NalazFajl,
                        Nalazsistematski = stav.Nalazsistematski,
                        UslugaNaziv = stav.UslugaNaziv,
                        RacunId = rac.RacunID
                    });

                    if (!string.IsNullOrEmpty(stav.Attachments))
                    {
                        string[] attachments = stav.Attachments.Split(";");
                        foreach (string attachment in attachments)
                        {
                            listaFajlova.Add(attachment);
                            fajlovi.Add(new FajloviTable { NazivFajla = attachment });
                        }
                    }
                }
            }

            // baca exception + premesti JSON u /GRESKA/ ako neki PDF fali
            _sftpService.PostojiFajl(listaFajlova, json.nazivJson);

            racun = HR_API.Helpers.FormatTypeHelper.ToDataTableFromList(racuni);
            stavka = HR_API.Helpers.FormatTypeHelper.ToDataTableFromList(stavke);
            fajl = HR_API.Helpers.FormatTypeHelper.ToDataTableFromList(fajlovi);

            return await _db.Procedures.DZOI_InsertSpecifikacijeRacunaFajlovaAsync(specifikacija, racun, stavka, fajl);
        }
    }
}
