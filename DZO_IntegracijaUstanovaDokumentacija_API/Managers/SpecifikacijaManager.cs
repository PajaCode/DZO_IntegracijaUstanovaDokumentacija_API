using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DomainClasses;
using HR_API.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{

    public class SpecifikacijaManager
    {
        private readonly GlobosSftpService _sftpService;
        private readonly VizimIntegracijaDb_Context _db;
        public SpecifikacijaManager(GlobosSftpService sftpService, VizimIntegracijaDb_Context db)
        {
            _sftpService = sftpService;
            _db = db;
        }

        public void UpisiFajlove()
        {
            List<string> listaFajlova = new();
            listaFajlova = _sftpService.ListaJsona();


            foreach (var file in listaFajlova)
            {
                var existingEntry = _db.DZOI_Vizim_Json.FirstOrDefault(x => x.nazivJson == file);
                if (existingEntry is null)
                {
                    DZOI_Vizim_Json newEntry = new DZOI_Vizim_Json
                    {
                        nazivJson = file,
                        StatusId = 1,
                        SistemskiDatum = DateTime.Now
                    };
                    _db.DZOI_Vizim_Json.Add(newEntry);
                    _db.SaveChanges();

                }
                else
                {
                    continue;
                }
            }
        }

        public async Task ParsirajIinsertujAsync()
        {
            var rezultat = _db.DZOI_Vizim_Json.Where(x => x.StatusId == 1).ToList();


            try
            {

                foreach (var rezultatItem in rezultat)
                {
                    string nazivString = new string(rezultatItem.nazivJson); // Konvertovanje niza char u string

                    // string putanjaDoFajla = Path.Combine(_sftpService.RemotePath, nazivString);

                    string sadrzajFajla = _sftpService.UzmiSadrzajFajla(nazivString);
                    _sftpService.Disconnect();
                    Logovi logovi = new(_db);
                    try
                    {
                        var jsonObject = JsonConvert.DeserializeObject<FileJson>(sadrzajFajla);

                        if(jsonObject is null)
                        {
                            logovi.LogError(rezultatItem.Id, "Izabrani JSON je prazan.");
                        }
                        else
                        {
                            try
                            {
                                _ = InsertPodatakaIzJsona(jsonObject, rezultatItem.Id);
                            }
                            catch (Exception ex)
                            {

                                logovi.LogError(rezultatItem.Id, ex.Message + "metoda parsirajIinsertuj - insert");
                                continue;
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {

                        logovi.LogError(rezultatItem.Id, ex.Message + "metoda parsirajIinsertuj - parsiranje JSON-a");
                        continue;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška: " + ex.Message);
                throw; // Bacaće originalnu grešku i možete videti tačan uzrok
            }
        }

        public async Task<List<DZOI_InsertSpecifikacijeRacunaFajlovaResult>> InsertPodatakaIzJsona(FileJson fileJson, int idJson)
        {

            DataTable specifikacija = new();
            DataTable racun = new();
            DataTable stavka = new();
            DataTable fajl = new();

            string opisJson = JsonConvert.SerializeObject(fileJson);
            ZaglavljeTable zaglavljeTable = new ZaglavljeTable
            {
                IdJson = idJson,
                FakturaId = fileJson.Zaglavlje.FakturaId,
                FakturaBroj = fileJson.Zaglavlje.FakturaBroj,
                UstanovaIDMG = fileJson.Zaglavlje.UstanovaIDMG,
                Datum = fileJson.Zaglavlje.Datum,
                OpisJson = opisJson
            };
            specifikacija = FormatTypeHelper.ToDataTableFromObject(zaglavljeTable);

            List<RacunTable> racuni = [];
            List<StavkaTable> stavke = [];
            List<FajloviTable> fajlovi = [];
            foreach (Racun rac in fileJson.Racun)
            {
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
                            FajloviTable fajloviTable = new FajloviTable
                            {
                                NazivFajla = attachment
                            };
                            fajlovi.Add(fajloviTable);
                        }
                    }
                }

            }
            racun = FormatTypeHelper.ToDataTableFromList(racuni);
            stavka = FormatTypeHelper.ToDataTableFromList(stavke);
            fajl = FormatTypeHelper.ToDataTableFromList(fajlovi);
            return await _db.Procedures.DZOI_InsertSpecifikacijeRacunaFajlovaAsync(specifikacija, racun, stavka, fajl);
        }




    }
}

