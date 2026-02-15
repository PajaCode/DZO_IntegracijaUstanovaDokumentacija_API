using DZO_IntegracijaUstanovaDokumentacija_API.Api;
using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
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

        // IdMetoda = 3
        public OperationSummary KreirajFoldere()
        {
            var result = (from specifikacija in _db.DZOI_Vizim_Specifikacija
                          join racun in _db.DZOI_Vizim_Racun on specifikacija.Id equals racun.IdSpecifikacije
                          where specifikacija.StatusId == 1
                          select new { specifikacija, brUputa = racun.UputBroj })
                          .ToList();

            var summary = new OperationSummary
            {
                Total = result.Count,
                Message = "Kreiranje foldera završeno."
            };

            using (_sftpService.ConnectScope())
            {
                foreach (var item in result)
                {
                    var brUputa = item.brUputa;
                    var idJson = Convert.ToInt32(item.specifikacija.IdJson);
                    var idSpec = Convert.ToInt32(item.specifikacija.Id);

                    try
                    {
                        var create = _sftpService.KreirajFolderNaSftp(brUputa);

                        if (!create.Succeeded) // AlreadyExists
                        {
                            _logger.AzurirajStatusspecifikacije(idJson, "pokušano kreiranje istog foldera", 3);
                            summary.Failed++;
                            summary.Errors.Add(new ItemError(idJson, idSpec, brUputa, "Folder već postoji"));
                            continue;
                        }

                        _db.DZOI_Vizim_Folder.Add(new DZOI_Vizim_Folder
                        {
                            IdJson = idJson,
                            nazivFoldera = brUputa,
                            StatusId = 1,
                            IdSpec = idSpec
                        });
                        _db.SaveChanges();

                        summary.Succeeded++;
                    }
                    catch (Exception ex)
                    {
                        _logger.AzurirajStatusspecifikacije(idJson, "desila se greška prilikom kreiranja foldera: " + ex.Message, 3);
                        summary.Failed++;
                        summary.Errors.Add(new ItemError(idJson, idSpec, brUputa, ex.Message));
                    }
                }
            }

            var status = summary.Failed == 0 ? ApiExecutionStatus.Succeeded : ApiExecutionStatus.CompletedWithErrors;
            return summary with { Status = status };
        }

        // IdMetoda = 4
        public async Task<OperationSummary> PrebaciFajloveAsync()
        {
            var fajlovi = (await _db.Procedures.DZOI_VratiFajloveZaPrebacivanjeAsync()).ToList();

            var summary = new OperationSummary
            {
                Total = fajlovi.Count,
                Message = "Prebacivanje fajlova završeno."
            };

            foreach (var grupa in fajlovi.GroupBy(f => f.IdJson))
            {
                using (_sftpService.ConnectScope())
                {
                    var stop = false;

                    foreach (var item in grupa)
                    {
                        if (stop) break;

                        var brUputa = item.UputBroj;
                        var idJson = Convert.ToInt32(item.IdJson);
                        var idSpec = Convert.ToInt32(item.IdSpec);
                        var nazivFajla = item.NazivFajla;

                        try
                        {
                            var move = _sftpService.PrebaciFajlUFolder(brUputa, nazivFajla);

                            if (!move.Succeeded)
                            {
                                _logger.LogError(idJson, move.ErrorMessage ?? "fajl ili folder ne postoji", idSpec, 4);
                                _logger.AzurirajStatusFajlova(idSpec, nazivFajla, 3);
                                _logger.AzurirajStatusspecifikacije(idJson, 3);
                                _logger.AzurirajStatusFoldera(idJson, brUputa, 1);

                                summary.Failed++;
                                summary.Errors.Add(new ItemError(idJson, idSpec, nazivFajla, move.ErrorMessage ?? "Neuspeh"));
                                stop = true; // prekini obradu ove grupe (isto ponašanje kao ranije)
                                continue;
                            }

                            _logger.AzurirajStatusFajlova(idSpec, nazivFajla, 2);
                            _logger.AzurirajStatusspecifikacije(idJson, 2);
                            _logger.AzurirajStatusFoldera(idJson, brUputa, 4);

                            summary.Succeeded++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(idJson, "desila se greška prilikom prebacivanja fajla: " + ex.Message, idSpec, 4);
                            _logger.AzurirajStatusFajlova(idSpec, nazivFajla, 3);
                            _logger.AzurirajStatusspecifikacije(idJson, 3);

                            summary.Failed++;
                            summary.Errors.Add(new ItemError(idJson, idSpec, nazivFajla, ex.Message));
                            stop = true;
                        }
                    }
                }
            }

            var status = summary.Failed == 0 ? ApiExecutionStatus.Succeeded : ApiExecutionStatus.CompletedWithErrors;
            return summary with { Status = status };
        }
    }
}