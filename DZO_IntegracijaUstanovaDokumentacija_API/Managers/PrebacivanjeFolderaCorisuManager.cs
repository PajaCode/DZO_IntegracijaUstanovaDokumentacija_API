using DZO_IntegracijaUstanovaDokumentacija_API.Api;
using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class PrebacivanjeFolderaCorisuManager
    {
        private readonly GlobosSftpService _globos;
        private readonly CorisSftpService _coris;
        private readonly RazmenaDokumentacijeDb_Context _db;
        private readonly Logovi _logger;

        public PrebacivanjeFolderaCorisuManager(GlobosSftpService globos, CorisSftpService coris, RazmenaDokumentacijeDb_Context db, Logovi logger)
        {
            _globos = globos;
            _coris = coris;
            _db = db;
            _logger = logger;
        }

        // IdMetoda = 5
        public OperationSummary PrebaciFoldere()
        {
            var folderi = _db.DZOI_Vizim_Folder.Where(d => d.StatusId == 4).ToList();

            var summary = new OperationSummary
            {
                Total = folderi.Count,
                Message = "Prebacivanje foldera na CORIS završeno."
            };

            foreach (var folder in folderi)
            {
                var naziv = folder.nazivFoldera;
                var idJson = Convert.ToInt32(folder.IdJson);
                var idSpec = Convert.ToInt32(folder.IdSpec);

                try
                {
                    using (_globos.ConnectScope())
                    using (_coris.ConnectScope())
                    {
                        var exists = _globos.ListaFoldera()
                            .Any(f => f.Equals(naziv, StringComparison.OrdinalIgnoreCase));

                        if (!exists)
                        {
                            _logger.LogError(idJson, "folder ne postoji na Globos SFTP-u", idSpec, 5);
                            _logger.AzurirajStatusFoldera(idJson, naziv, 3);
                            summary.Failed++;
                            summary.Errors.Add(new ItemError(idJson, idSpec, naziv, "Folder ne postoji na Globos SFTP-u"));
                            continue;
                        }

                        var transfer = _coris.PrebaciFolderNaCoris(naziv);

                        if (!transfer.Succeeded)
                        {
                            _logger.LogError(idJson, "nije se kopirao folder: " + transfer.ErrorMessage, idSpec, 5);
                            _logger.AzurirajStatusFoldera(idJson, naziv, 3);
                            summary.Failed++;
                            summary.Errors.Add(new ItemError(idJson, idSpec, naziv, transfer.ErrorMessage ?? "Neuspeh"));
                        }
                        else
                        {
                            _logger.AzurirajStatusFoldera(idJson, naziv, 2);
                            summary.Succeeded++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(idJson, "nije se kopirao folder: " + ex.Message, idSpec, 5);
                    summary.Failed++;
                    summary.Errors.Add(new ItemError(idJson, idSpec, naziv, ex.Message));
                }
            }

            var status = summary.Failed == 0 ? ApiExecutionStatus.Succeeded : ApiExecutionStatus.CompletedWithErrors;
            return summary with { Status = status };
        }
    }
}