using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DomainClasses;
using HR_API.Helpers;
using System.Net.Http.Json;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class SpecifikacijaManager(VizimIntegracijaDb_TestContext db)
    {
        public List<InsertedFile> Files (List<FileDetail> files)
        {
            List<InsertedFile> fileNameList = new();
         
            foreach (FileDetail file in files)
            {
                var existingEntry = db.DZOI_Vizim_Json.FirstOrDefault(x => x.nazivJson == file.Name);
                if (existingEntry is null)
                {
                    DZOI_Vizim_Json newEntry = new DZOI_Vizim_Json
                    {
                        nazivJson = file.Name,
                        StatusId = 1,
                        SistemskiDatum = DateTime.Now
                    };
                    db.DZOI_Vizim_Json.Add(newEntry);
                    db.SaveChanges();
                    InsertedFile newFile = new InsertedFile
                    {
                        IdJson = newEntry.Id,
                        FullName = file.FullName
                    };
                    fileNameList.Add(newFile);
                }
                else
                {
                    continue;
                }
            }

            return fileNameList;    
        }
        public FileJson ReadJson(string jsonContent)
        {
            return JsonConvert.DeserializeObject<FileJson>(jsonContent);
        }

        public void LogError(int fileId, string error)
        {
            DZOI_Vizim_Json jSon = db.DZOI_Vizim_Json.Where(j => j.Id == fileId).FirstOrDefault();
            jSon.StatusId = 3;
            db.DZOI_Vizim_ErrorJson.Add(new DZOI_Vizim_ErrorJson
            {
                IdJson = fileId,
                NazivGreske = error
            });
            db.SaveChanges();
        }


        public async Task<List<DZOI_InsertSpecifikacijeRacunaFajlovaResult>> InsertPodatakaIzJsona(FileJson fileJson,int idJson)
        {

            DataTable specifikacija = new();
            DataTable racun = new();
            DataTable stavka = new();
            DataTable fajl = new();

            string opisJson = JsonConvert.SerializeObject(fileJson);
            ZaglavljeTable zaglavljeTable = new ZaglavljeTable
            {   
                IdJson= idJson,
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
            return  await db.Procedures.DZOI_InsertSpecifikacijeRacunaFajlovaAsync(specifikacija, racun, stavka, fajl);
        }
    }
}
