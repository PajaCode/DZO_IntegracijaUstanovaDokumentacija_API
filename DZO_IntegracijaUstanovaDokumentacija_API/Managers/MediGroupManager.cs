using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using HR_API.Helpers;
namespace DZO_IntegracijaUstanovaDokumentacija_API.Managers
{
    public class MediGroupManager
    {
        private readonly MediGroupSftpService _sftpService;
        private readonly RazmenaDokumentacijeDb_Context _db;
        private readonly CorisSftpService _sftpServiceCor;
        private readonly Logovi _logger;
        public MediGroupManager(MediGroupSftpService sftpService, CorisSftpService sftpServiceCor,RazmenaDokumentacijeDb_Context db, Logovi logger)
        {
            _sftpService = sftpService;
            _db = db;
            _logger = logger;
            _sftpServiceCor=sftpServiceCor;
        }


        public void UpisiZip()
        {
            List<string> listZip = _sftpService.ListaZipFajlova();

            foreach (var file in listZip)
            {
                var existingEntry = _db.DZOI_MediGroup_Zip.FirstOrDefault(x => x.nazivZip == file);
                if (existingEntry is null)
                {
                    DZOI_MediGroup_Zip newEntry = new DZOI_MediGroup_Zip
                    {
                        nazivZip = file,
                        StatusId = 1,                      
                    };
                    _db.DZOI_MediGroup_Zip.Add(newEntry);
                    _db.SaveChanges();

                }
                else
                {
                    continue;
                }
            }
        }

        public void PrebaciZipFajlove()
        {

            List<DZOI_MediGroup_Zip> zipFolderi = _db.DZOI_MediGroup_Zip.Where(mg => mg.StatusId == 1).ToList();
            foreach (var zip in zipFolderi)
            {
                int idZip = Convert.ToInt32(zip.Id);
                List<string> listZip = _sftpService.ListaZipFajlova();
                _sftpService.Connect();
                _sftpServiceCor.Connect();
                foreach (string file in listZip)
                {
                    var uspeh = _sftpService.PrebaciZipSFTP(file);
                    if (uspeh[0] == "Neuspeh")
                    {
                        _logger.LogErrorZip(idZip, "Zip fajl nije prebačen");
                        _logger.AzurirajsStatusZipa(idZip, 3);                        
                        continue;
                    }
                    else
                    {
                        _logger.AzurirajsStatusZipa(idZip, 2);
                    }
                }
                _sftpService.Disconnect();
                _sftpServiceCor.Disconnect();
            }
            
        }
    }
}
