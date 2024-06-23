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
                    string uspeh = _sftpService.KreirajFolderNaSFTP(brUputa);
                    if (uspeh == "Neuspeh") { logovi.LogError(IdJson, "pokusano kreiranje istog foldera");  }
                    else
                    {
                        var noviRed = new DZOI_Vizim_Folder
                        {
                            nazivFoldera = brUputa,
                            StatusId = 1
                        };
                        _db.DZOI_Vizim_Folder.Add(noviRed);
                        _db.SaveChanges();


                    }
                }
                catch (Exception ex)
                {

                    logovi.LogError(IdJson, "desila se greska prilikom kreiranja foldera:"+ex);
                }
            }
        }
        public  void prebaciFajloveAsync()
        {     
             List < DZOI_VratiFajloveZaPrebacivanjeResult > fajlovi =  _db.Procedures.DZOI_VratiFajloveZaPrebacivanjeAsync().Result.ToList();

            foreach (var item in fajlovi)
            {
                string brUputa = new string(item.UputBroj);
                int IdJson = Convert.ToInt32(item.IdJson);
                int idSpec = Convert.ToInt32(item.IdSpec);
                string NazivFajla = new string(item.NazivFajla);
                Logovi logovi = new(_db);
                try
                {
                    var uspeh =  _sftpService.PrebaciFajlove(brUputa,NazivFajla);
                    if (uspeh == "Neuspeh") { logovi.LogError(IdJson, "fajl ne postoji"); logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 3); }
                    else { logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 2);  }
                }
                catch (Exception ex)
                {

                    logovi.LogError(IdJson, "desila se greska prilikom prebacivanja fajla:" + ex);
                    logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 3);
                }

            }

        }
    }
}
