using Microsoft.EntityFrameworkCore.Storage;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public class Logovi
    {
       
        private readonly VizimIntegracijaDb_Context _db;
        public Logovi( VizimIntegracijaDb_Context db)
        {
            _db = db;
        }
        public  void LogError(int fileId, string error , int IdSpec , int IdMetoda)
        {   
            DZOI_Vizim_Json jSon = _db.DZOI_Vizim_Json.Where(j => j.Id == fileId).FirstOrDefault();
            jSon.StatusId = 3;
            _db.DZOI_Vizim_ErrorJson.Add(new DZOI_Vizim_ErrorJson
            {
                IdJson = fileId,
                NazivGreske = error,
                IdSpec = IdSpec,
                IdMetoda = IdMetoda
            });
            _db.SaveChanges();
        }

        public void AzurirajStatusVizimJson(int IdJson, int status)
        {
            var redoviZaAzuriranje = _db.DZOI_Vizim_Json.Where(d => d.Id == IdJson).ToList();

            foreach (var red in redoviZaAzuriranje)
            {
                red.StatusId = status;
                
            }

            _db.SaveChanges();

        }

        public void AzurirajStatusFajlova(int IdSpec, string NazivFajla, int status)
        {
            var redoviZaAzuriranje = _db.DZOI_Vizim_Fajlovi.Where(d => d.IdSpecifikacije == IdSpec && d.NazivFajla == NazivFajla).ToList();

            foreach (var red in redoviZaAzuriranje)
            {
                red.StatusId =status;
                red.DatumPrebacivanja = DateTime.Now;
            }

            _db.SaveChanges();

        }

        public void AzurirajStatusFoldera(int IdJson, string NazivFoldera, int status)
        {
            var redoviZaAzuriranje = _db.DZOI_Vizim_Folder.Where(d => d.IdJson == IdJson && d.nazivFoldera == NazivFoldera).ToList();

            foreach (var red in redoviZaAzuriranje)
            {
                red.StatusId = status;
                red.DatumPrebacivanja = DateTime.Now;
            }

            _db.SaveChanges();

        }


        public void AzurirajStatusspecifikacije(int IdJson, string error , int IdMetoda)
        {
            var redoviZaAzuriranje = _db.DZOI_Vizim_Specifikacija.Where(d => d.IdJson == IdJson ).ToList();

            foreach (var red in redoviZaAzuriranje)
            {
                red.StatusId = 3;
                
            }


            _db.DZOI_Vizim_ErrorJson.Add(new DZOI_Vizim_ErrorJson
            {
                IdJson = IdJson,
                NazivGreske = error,
                IdMetoda = IdMetoda
            });
            _db.SaveChanges();

            

        }

        public void AzurirajStatusspecifikacije(int IdJson, int status)
        {
            var redoviZaAzuriranje = _db.DZOI_Vizim_Specifikacija.Where(d => d.IdJson == IdJson).ToList();

            foreach (var red in redoviZaAzuriranje)
            {
                red.StatusId = status;

            }
            _db.SaveChanges();

        }
    }
}

