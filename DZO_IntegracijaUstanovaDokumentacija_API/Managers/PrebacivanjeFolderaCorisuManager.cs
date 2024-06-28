using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class PrebacivanjeFolderaCorisuManager
    {
        private readonly GlobosSftpService _sftpService;
        private readonly CorisSftpService _sftpServiceCor;
        private readonly VizimIntegracijaDb_Context _db;
        private readonly Logovi _logger;
        public PrebacivanjeFolderaCorisuManager(GlobosSftpService sftpService, CorisSftpService sftpServiceCor, VizimIntegracijaDb_Context db, Logovi logger)
        {
            _sftpService = sftpService;
            _sftpServiceCor = sftpServiceCor;
            _db = db;
            _logger = logger;
        }

        public void PrebaciFoldere()
        {
            var NazivFoldera = _db.DZOI_Vizim_Folder.Where(d => d.StatusId == 1).ToList();
           
            foreach (var folderzaslanje in NazivFoldera)
            {
                string naziv = new string(folderzaslanje.nazivFoldera);
                int IdJson =  Convert.ToInt32(folderzaslanje.IdJson);

                try
                {

                    _sftpService.Connect();

                    _sftpServiceCor.Connect();

                    var listaFoldera = _sftpService.ListaFoldera();

                    foreach (var folder in listaFoldera)
                    {

                        if (folder.Equals(naziv, StringComparison.OrdinalIgnoreCase))
                        {

                            var uspeh = _sftpServiceCor.PrebaciFoldereSFTP(naziv, folder);
                            if (uspeh[0] == "Neuspeh") {
                                _logger.LogError(IdJson, "nije se kopirao folder:" + uspeh[1]); _logger.AzurirajStatusFoldera(IdJson, naziv, 3);//kopiraj u gresku
                                continue; }
                            else {
                                _logger.AzurirajStatusFoldera(IdJson, naziv, 2); //brisi folder
                            }
                        } 
                    }

                }
                catch (Exception ex)
                {

                    _logger.LogError(IdJson, "nije se kopirao folder:" + ex.ToString());
                }
            }

         

               

        }
    }
}
