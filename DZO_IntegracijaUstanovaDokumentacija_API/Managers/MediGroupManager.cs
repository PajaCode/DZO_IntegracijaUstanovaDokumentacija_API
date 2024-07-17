using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class MediGroupManager
    {
        private readonly MediGroupSftpService _sftpService;
        public MediGroupManager(MediGroupSftpService sftpService)
        {
            _sftpService = sftpService;
        }


        public void PrebaciZipFajlove()
        {
            List<string> listZip = _sftpService.ListaZipFajlova();
            _sftpService.Connect();
            foreach (string zip in listZip)
            {
                var uspeh=_sftpService.PrebaciZipSFTP(zip);
                if (uspeh[0] == "Neuspeh")            
                {            
                    continue;
                }               
            }
        }
    }
}
