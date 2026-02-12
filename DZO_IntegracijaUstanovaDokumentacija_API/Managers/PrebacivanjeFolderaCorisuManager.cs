using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class PrebacivanjeFolderaCorisuManager
    {
        private readonly GlobosSftpService _sftpService;
        private readonly CorisSftpService _sftpServiceCor;
        private readonly RazmenaDokumentacijeDb_Context _db;
        private readonly Logovi _logger;
        public PrebacivanjeFolderaCorisuManager(GlobosSftpService sftpService, CorisSftpService sftpServiceCor, RazmenaDokumentacijeDb_Context db, Logovi logger)
        {
            _sftpService = sftpService;
            _sftpServiceCor = sftpServiceCor;
            _db = db;
            _logger = logger;
        }

        public void PrebaciFoldere()
        {
            _db.DZOI_Vizim_Folder.Where(d => d.StatusId == 4).ToList().ForEach(folderzaslanje =>
            {
                var naziv = new string(folderzaslanje.nazivFoldera);
                var IdJson = Convert.ToInt32(folderzaslanje.IdJson);
                var IdSpec = Convert.ToInt32(folderzaslanje.IdSpec);

                try
                {
                    using (_sftpService.ConnectScope())
                    using (_sftpServiceCor.ConnectScope())
                    {
                        _sftpService.ListaFoldera()
                            .Where(f => f.Equals(naziv, StringComparison.OrdinalIgnoreCase))
                            .Take(1) // samo taj folder
                            .ToList()
                            .ForEach(_ =>
                            {
                                var uspeh = _sftpServiceCor.PrebaciFoldereSFTP(naziv);
                                if (uspeh[0] == "Neuspeh")
                                {
                                    _logger.LogError(IdJson, "nije se kopirao folder:" + uspeh[1], IdSpec, 5);
                                    _logger.AzurirajStatusFoldera(IdJson, naziv, 3);
                                }
                                else
                                {
                                    _logger.AzurirajStatusFoldera(IdJson, naziv, 2);
                                }
                            });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(IdJson, "nije se kopirao folder:" + ex.ToString(), IdSpec, 5);
                }
            });
        }
    }
}