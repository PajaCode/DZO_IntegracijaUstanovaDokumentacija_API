using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class PrebacivanjeFajlovaManager
    {
        private readonly GlobosSftpService _sftpService;
        private readonly VizimIntegracijaDb_Context _db;
        private readonly Logovi _logger;
        public PrebacivanjeFajlovaManager(GlobosSftpService sftpService, VizimIntegracijaDb_Context db, Logovi logger)
        {
            _sftpService = sftpService;
            _db = db;
            _logger = logger;   
        }


        public void KreirajFoldere()
        {
            var rezultat = (from specifikacija in _db.DZOI_Vizim_Specifikacija
                           join racun in _db.DZOI_Vizim_Racun on specifikacija.Id equals racun.IdSpecifikacije
                           where specifikacija.StatusId == 1
            select new
            {
              specifikacija,
              brUputa = racun.UputBroj
            }).ToList();

            foreach (var item in rezultat)
            {
                string brUputa = new string(item.brUputa);
                int IdJson = Convert.ToInt32(item.specifikacija.IdJson);
               
                try
                {  
                    string uspeh = _sftpService.KreirajFolderNaSFTP(brUputa);
                    _sftpService.Disconnect();
                    if (uspeh == "Neuspeh") 
                    { 
                        _logger.AzurirajStatusspecifikacije(IdJson, "pokusano kreiranje istog foldera");  
                    }
                    else
                    {
                        var noviRed = new DZOI_Vizim_Folder
                        {
                            IdJson= IdJson,
                            nazivFoldera = brUputa,
                            StatusId = 1
                        };
                        _db.DZOI_Vizim_Folder.Add(noviRed);
                        _db.SaveChanges();


                    }
                }
                catch (Exception ex)
                {

                    _logger.AzurirajStatusspecifikacije(IdJson, "desila se greska prilikom kreiranja foldera:"+ex);
                }
            }
        }
        public  void PrebaciFajloveAsync()
        {     
             List < DZOI_VratiFajloveZaPrebacivanjeResult > fajlovi =  _db.Procedures.DZOI_VratiFajloveZaPrebacivanjeAsync().Result.ToList();

            foreach (var item in fajlovi)
            {
                string brUputa = new string(item.UputBroj);
                int IdJson = Convert.ToInt32(item.IdJson);
                int idSpec = Convert.ToInt32(item.IdSpec);
                string NazivFajla = new string(item.NazivFajla);
             
                try
                {
                    var uspeh =  _sftpService.PrebaciFajlove(brUputa,NazivFajla);
                    if (uspeh == "Neuspeh") { _logger.LogError(IdJson, "fajl ne postoji"); _logger.AzurirajStatusFajlova(idSpec, NazivFajla, 3); }
                    else { _logger.AzurirajStatusFajlova(idSpec, NazivFajla, 2);  }
                }
                catch (Exception ex)
                {

                    _logger.LogError(IdJson, "desila se greska prilikom prebacivanja fajla:" + ex);
                    _logger.AzurirajStatusFajlova(idSpec, NazivFajla, 3);
                }

            }

        }
    }
}
