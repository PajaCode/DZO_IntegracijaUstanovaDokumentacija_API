using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using Microsoft.EntityFrameworkCore;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class PrebacivanjeFajlovaManager
    {
        private readonly GlobosSftpService _sftpService;
        private readonly VizimIntegracijaDb_Context _db;
        public PrebacivanjeFajlovaManager(GlobosSftpService sftpService, VizimIntegracijaDb_Context db)
        {
            _sftpService = sftpService;
            _db = db;
        }


        public void kreirajFoldere()
        {
            var rezultat = from specifikacija in _db.DZOI_Vizim_Specifikacija
                           join racun in _db.DZOI_Vizim_Racun on specifikacija.Id equals racun.IdSpecifikacije
                           where specifikacija.StatusId == 1
            select new
            {
              specifikacija,
              brUputa = racun.UputBroj
            };

            foreach (var item in rezultat)
            {
                 string brUputa = new string(item.brUputa);
                int IdJson = Convert.ToInt32(item.specifikacija.Id);
                Logovi logovi = new(_db);
                try
                {  
                    string uspeh = _sftpService.KreirajFolderNaSFTP(brUputa, IdJson);
                    if (uspeh == "Neuspeh") { logovi.LogError(IdJson, "pokusano kreiranje istog foldera"); ; }
                }
                catch (Exception ex)
                {

                    

                    logovi.LogError(IdJson, "desila se greska prilikom kreiranja foldera:"+ex); ;
                }
            }
        }
        public async Task prebaciFajloveAsync()
        {
             var fajlovi = await _db.Procedures.DZOI_VratiFajloveZaPrebacivanjeAsync();

            foreach (var item in fajlovi)
            {
                string brUputa = new string(item.UputBroj);
                int IdSpec = Convert.ToInt32(item.IdSpec);
                string NazivFajla = new string(item.NazivFajla);
                Logovi logovi = new(_db);
                try
                {
                     _sftpService.PrebaciFajlove(brUputa, IdSpec ,NazivFajla);
                    //if (uspeh == "Neuspeh") { logovi.LogError(IdJson, "pokusano kreiranje istog foldera"); ; }
                }
                catch (Exception ex)
                {



                    //logovi.LogError(IdJson, "desila se greska prilikom kreiranja foldera:" + ex); ;
                }

            }

        }
    }
}
