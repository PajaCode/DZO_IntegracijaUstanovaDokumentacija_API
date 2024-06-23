using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class PrebacivanjeFolderaCorisuManager
    {
        private readonly GlobosSftpService _sftpService;
        private readonly CorisSftpService _sftpServiceCor;
        private readonly VizimIntegracijaDb_Context _db;
        public PrebacivanjeFolderaCorisuManager(GlobosSftpService sftpService, CorisSftpService sftpServiceCor, VizimIntegracijaDb_Context db)
        {
            _sftpService = sftpService;
            _sftpServiceCor = sftpServiceCor;
            _db = db;
        }

        public void PrebaciFoldere()
        {
            var NazivFoldera = _db.DZOI_Vizim_Folder.Where(d => d.StatusId == 1).ToList();
            Logovi logovi = new(_db);
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
                            if (uspeh[0] == "Neuspeh") { logovi.LogError(IdJson, "nije se kopirao folder:" + uspeh[1]); logovi.AzurirajStatusFoldera(IdJson, naziv, 3); }
                            else { logovi.AzurirajStatusFoldera(IdJson, naziv, 2); //brisi folder
                            }
                        } 
                    }

                }
                catch (Exception ex)
                {

                    throw;
                }
            }

               

        }
    }
}
