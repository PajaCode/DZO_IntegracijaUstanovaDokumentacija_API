
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DomainClasses;
using HR_API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecifikacijaController(VizimIntegracijaDb_TestContext db): ControllerBase
    {
        private SpecifikacijaManager spec = new(db);
        private readonly string host = "192.168.20.40";
        private readonly string username = "test";
        private readonly string password = "G10b05BG";
        private readonly string remotePath = "/home/test";

        [HttpGet("files")]
        public async Task<IActionResult> GetFilesAsync()
        {
            try
            {
                using SftpClient sftp = new(host, username, password);
                sftp.Connect();

                var files = sftp.ListDirectory(remotePath)
                                .Where(f => f.IsRegularFile && f.Name.EndsWith(".json"))
                                .Select(f => new { f.Name, f.FullName, f.LastWriteTime })
                                .Take(5)
                                .ToList();

                List<FileJson> jsonFileList = [];

                foreach (var file in files)
                {
                    string sourceFilePath = file.FullName;

                    // Čitanje sadržaja JSON fajla direktno iz memorije
                    using (MemoryStream stream = new MemoryStream())
                    {
                        sftp.DownloadFile(sourceFilePath, stream);
                        stream.Position = 0;

                        using (var reader = new StreamReader(stream))
                        {
                            string jsonContent = reader.ReadToEnd();
                            FileJson jsonObject = JsonConvert.DeserializeObject<FileJson>(jsonContent);
                            jsonFileList.Add(jsonObject);
                        }
                        
                    } 
                   
                }

                sftp.Disconnect();

                DataTable specifikacija = new();
                DataTable racun=new();
                DataTable stavka=new();
                DataTable fajl=new();
                foreach (FileJson file in jsonFileList)
                {
                    string opisJson=JsonConvert.SerializeObject(file);
                    ZaglavljeTable zaglavljeTable = new ZaglavljeTable
                    {
                        FakturaId=file.Zaglavlje.FakturaId,
                        FakturaBroj=file.Zaglavlje.FakturaBroj,
                        UstanovaIDMG=file.Zaglavlje.UstanovaIDMG,
                        Datum=file.Zaglavlje.Datum,
                        OpisJson=opisJson
                    };
                    specifikacija = FormatTypeHelper.ToDataTableFromObject(zaglavljeTable);

                    List<RacunTable> racuni = [];
                    List<StavkaTable> stavke = [];
                    List<FajloviTable> fajlovi = [];
                    foreach(Racun rac in file.Racun)
                    {
                        RacunTable racunTable = new RacunTable
                        {
                            RacunID = rac.RacunID,
                            RacunDatum=rac.RacunDatum,
                            RacunBrojFiskala=rac.RacunBrojFiskala,
                            UputBroj=rac.UputBroj,
                            BrojKartice = rac.PacijentID,
                            UkupanIznos=rac.IZNOSCLAIM,
                            RacunFajl=rac.RacunFajl,
                            UputFajl=rac.UputFajl,
                        };
                        racuni.Add( racunTable );

                        foreach(Stavka stav in rac.Stavka)
                        {
                            StavkaTable stavkaTable = new StavkaTable
                            {
                                StavkaID=stav.StavkaID,
                                UslugaDatum=stav.UslugaDatum,
                                Popust=stav.Popust,
                                PunaCena=stav.PunaCena,
                                ZaIsplatu=stav.ZaUplatu,
                                Valuta=stav.Valuta,
                                UslugaID=stav.UslugaID,
                                SARADNIKID=stav.SARADNIKID,
                                SARADNIKNAZIV=stav.SARADNIKNAZIV,
                                LabNalazFajl=stav.LabNalazFajl,
                                NalazFajl= stav.NalazFajl,
                                Nalazsistematski=stav.Nalazsistematski,
                                UslugaNaziv=stav.UslugaNaziv,
                                RacunId=rac.RacunID
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
                    racun=FormatTypeHelper.ToDataTableFromList( racuni );
                    stavka = FormatTypeHelper.ToDataTableFromList(stavke);
                    fajl= FormatTypeHelper.ToDataTableFromList(fajlovi);
                    var promena= await db.Procedures.DZOI_InsertSpecifikacijeRacunaFajlovaAsync(specifikacija, racun, stavka, fajl);
                }

                return Ok("Dobar posao odrađen");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
