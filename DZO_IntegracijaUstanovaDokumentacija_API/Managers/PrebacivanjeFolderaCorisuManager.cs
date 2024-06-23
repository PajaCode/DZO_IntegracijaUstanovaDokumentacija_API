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

            foreach (var folder in NazivFoldera)
            {
                string naziv = new string(folder.nazivFoldera);

                try
                {
                    var uspeh = _sftpServiceCor.PrebaciFoldere(naziv);
                    //        if (uspeh == "Neuspeh") { logovi.LogError(IdJson, "fajl ne postoji"); logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 3); }
                    //        else { logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 2); }

                }
                catch (Exception)
                {

                    throw;
                }
            }

                //foreach (var item in fajlovi)
                //{
                //    string brUputa = new string(item.UputBroj);
                //    int IdJson = Convert.ToInt32(item.IdJson);
                //    int idSpec = Convert.ToInt32(item.IdSpec);
                //    string NazivFajla = new string(item.NazivFajla);
                //    Logovi logovi = new(_db);
                //    try
                //    {
                //        var uspeh = _sftpService.PrebaciFajlove(brUputa, NazivFajla);
                //        if (uspeh == "Neuspeh") { logovi.LogError(IdJson, "fajl ne postoji"); logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 3); }
                //        else { logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 2); }
                //    }
                //    catch (Exception ex)
                //    {

                //        logovi.LogError(IdJson, "desila se greska prilikom prebacivanja fajla:" + ex);
                //        logovi.AzurirajStatusFajlova(idSpec, NazivFajla, 3);
                //    }

                //}

        }
    }
}
