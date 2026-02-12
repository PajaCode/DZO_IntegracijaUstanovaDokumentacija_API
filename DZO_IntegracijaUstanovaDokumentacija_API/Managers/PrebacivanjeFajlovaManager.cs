using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class PrebacivanjeFajlovaManager
    {
        private readonly GlobosSftpService _sftpService;
        private readonly RazmenaDokumentacijeDb_Context _db;
        private readonly Logovi _logger;
        public PrebacivanjeFajlovaManager(GlobosSftpService sftpService, RazmenaDokumentacijeDb_Context db, Logovi logger)
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
                            select new { specifikacija, brUputa = racun.UputBroj })
                           .ToList();

            using (_sftpService.ConnectScope())
            {
                rezultat.ForEach(item =>
                {
                    var brUputa = new string(item.brUputa);
                    var IdJson = Convert.ToInt32(item.specifikacija.IdJson);
                    var IdSpec = Convert.ToInt32(item.specifikacija.Id);

                    try
                    {
                        var uspeh = _sftpService.KreirajFolderNaSFTP(brUputa);
                        if (uspeh == "Neuspeh")
                        {
                            _logger.AzurirajStatusspecifikacije(IdJson, "pokusano kreiranje istog foldera", 3);
                        }
                        else
                        {
                            _db.DZOI_Vizim_Folder.Add(new DZOI_Vizim_Folder
                            {
                                IdJson = IdJson,
                                nazivFoldera = brUputa,
                                StatusId = 1,
                                IdSpec = IdSpec
                            });
                            _db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.AzurirajStatusspecifikacije(IdJson, "desila se greska prilikom kreiranja foldera:" + ex, 3);
                    }
                });
            }
        }
        public void PrebaciFajloveAsync()
        {
            var fajlovi = _db.Procedures.DZOI_VratiFajloveZaPrebacivanjeAsync().Result.ToList();

            fajlovi.GroupBy(f => f.IdJson).ToList().ForEach(grupa =>
            {
                using (_sftpService.ConnectScope())
                {
                    var stop = false;

                    grupa.TakeWhile(_ => !stop).ToList().ForEach(item =>
                    {
                        var brUputa = new string(item.UputBroj);
                        var IdJson = Convert.ToInt32(item.IdJson);
                        var idSpec = Convert.ToInt32(item.IdSpec);
                        var NazivFajla = new string(item.NazivFajla);

                        try
                        {
                            var uspeh = _sftpService.PrebaciFajlove(brUputa, NazivFajla);
                            if (!uspeh)
                            {
                                _logger.LogError(IdJson, "fajl ili folder ne postoji", idSpec, 4);
                                _logger.AzurirajStatusFajlova(idSpec, NazivFajla, 3);
                                _logger.AzurirajStatusspecifikacije(IdJson, 3);
                                _logger.AzurirajStatusFoldera(IdJson, brUputa, 1);
                                stop = true; // emulira 'break'
                            }
                            else
                            {
                                _logger.AzurirajStatusFajlova(idSpec, NazivFajla, 2);
                                _logger.AzurirajStatusspecifikacije(IdJson, 2);
                                _logger.AzurirajStatusFoldera(IdJson, brUputa, 4);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(IdJson, "desila se greska prilikom prebacivanja fajla:" + ex, idSpec, 4);
                            _logger.AzurirajStatusFajlova(idSpec, NazivFajla, 3);
                            _logger.AzurirajStatusspecifikacije(IdJson, 3);
                        }
                    });
                }
            });
        }
    }
}