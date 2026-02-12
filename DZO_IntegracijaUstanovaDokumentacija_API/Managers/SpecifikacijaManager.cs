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

        public void UpisiJsone()
        {
            var sb = new StringBuilder();
            var listaFajlova = _sftpService.ListaJsona();

            using (_sftpService.ConnectScope())
            {
                listaFajlova.ForEach(file =>
                {
                    var zaPretragu = file.Substring(0, file.IndexOf('.'));
                    var existsCount = _db.DZOI_Vizim_Json.Count(x => x.nazivJson.Contains(zaPretragu) && x.StatusId == 2);

                    if (existsCount == 0)
                    {
                        _db.DZOI_Vizim_Json.Add(new DZOI_Vizim_Json
                        {
                            nazivJson = file,
                            StatusId = 1,
                            SistemskiDatum = DateTime.Now
                        });
                        _db.SaveChanges();
                    }
                    else
                    {
                        sb.Clear();
                        sb.Append(file).Replace(".JSON", "").Append('_').Append(existsCount).Append(".JSON");

                        var root = _sftpService.VratiPutanju();
                        _sftpService._sftpClient.RenameFile($"{root}/{file}", $"{root}/{sb}");

                        _db.DZOI_Vizim_Json.Add(new DZOI_Vizim_Json
                        {
                            nazivJson = sb.ToString(),
                            StatusId = 1,
                            SistemskiDatum = DateTime.Now
                        });
                        _db.SaveChanges();
                    }
                });
            }
        }

        public async Task ParsirajIinsertujAsync()
        {
            var rezultat = _db.DZOI_Vizim_Json.Where(x => x.StatusId == 1).ToList();
            var settings = new JsonSerializerSettings
            {
                Culture = CultureInfo.GetCultureInfo("sr-Latn-RS")
            };

            try
            {

                foreach (var rezultatItem in rezultat)
                {
                    string nazivString = new string(rezultatItem.nazivJson);

                    string sadrzajFajla = _sftpService.UzmiSadrzajFajla(nazivString);
                    _sftpService.Disconnect();

                    try
                    {

                        var jsonObject = JsonConvert.DeserializeObject<FileJson>(sadrzajFajla, settings);

                        if (jsonObject is null)
                        {
                            _logger.LogError(rezultatItem.Id, "Izabrani JSON je prazan.", 0, 2);
                            continue;
                        }
                        else
                        {
                            try
                            {
                                _ = await InsertPodatakaIzJsona(jsonObject, rezultatItem);
                                _logger.AzurirajStatusVizimJson(rezultatItem.Id, 2);
                            }
                            catch (Exception ex)
                            {

                                _logger.LogError(rezultatItem.Id, ex.Message + "metoda parsirajIinsertuj - insert", 0, 2);
                                continue;
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(rezultatItem.Id, ex.Message + "metoda parsirajIinsertuj - parsiranje JSON-a", 0, 2);
                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška: " + ex.Message);
                throw;
            }
        }

        public async Task<List<DZOI_InsertSpecifikacijeRacunaFajlovaResult>> InsertPodatakaIzJsona(FileJson fileJson, DZOI_Vizim_Json json)
        {

            DataTable specifikacija = new();
            DataTable racun = new();
            DataTable stavka = new();
            DataTable fajl = new();
            List<string> listaFajlova = new();
            string opisJson = JsonConvert.SerializeObject(fileJson);

            ZaglavljeTable zaglavljeTable = new ZaglavljeTable
            {
                IdJson = json.Id,
                FakturaId = fileJson.Zaglavlje.FakturaId,
                FakturaBroj = fileJson.Zaglavlje.FakturaBroj,
                UstanovaIDMG = fileJson.Zaglavlje.UstanovaIDMG,
                Datum = fileJson.Zaglavlje.Datum,
                OpisJson = opisJson
            };
            specifikacija = FormatTypeHelper.ToDataTableFromObject(zaglavljeTable);

            List<RacunTable> racuni = new();
            List<StavkaTable> stavke = new();
            List<FajloviTable> fajlovi = new();
            foreach (Racun rac in fileJson.Racun)
            {
                if (!String.IsNullOrEmpty(rac.RacunFajl))
                {
                    listaFajlova.Add(rac.RacunFajl);
                }
                if (!String.IsNullOrEmpty(rac.UputFajl))
                {
                    listaFajlova.Add(rac.UputFajl);
                }

                RacunTable racunTable = new RacunTable
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
                };
                racuni.Add(racunTable);

                foreach (Stavka stav in rac.Stavka)
                {

                    if (!String.IsNullOrEmpty(stav.LabNalazFajl))
                    {
                        listaFajlova.Add(stav.LabNalazFajl);
                    }
                    if (!String.IsNullOrEmpty(stav.NalazFajl))
                    {
                        listaFajlova.Add(stav.NalazFajl);
                    }
                    if (!String.IsNullOrEmpty(stav.Nalazsistematski))
                    {
                        listaFajlova.Add(stav.Nalazsistematski);
                    }


                    StavkaTable stavkaTable = new StavkaTable
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
                    };
                    stavke.Add(stavkaTable);
                    if (!String.IsNullOrEmpty(stav.Attachments))
                    {
                        string[] attachments = stav.Attachments.Split(";");
                        foreach (string attachment in attachments)
                        {
                            listaFajlova.Add(attachment);
                            FajloviTable fajloviTable = new FajloviTable
                            {
                                NazivFajla = attachment
                            };
                            fajlovi.Add(fajloviTable);
                        }
                    }
                }

            }

            _sftpService.PostojiFajl(listaFajlova, json.nazivJson);

            racun = FormatTypeHelper.ToDataTableFromList(racuni);
            stavka = FormatTypeHelper.ToDataTableFromList(stavke);
            fajl = FormatTypeHelper.ToDataTableFromList(fajlovi);
            return await _db.Procedures.DZOI_InsertSpecifikacijeRacunaFajlovaAsync(specifikacija, racun, stavka, fajl);
        }
    }
}
