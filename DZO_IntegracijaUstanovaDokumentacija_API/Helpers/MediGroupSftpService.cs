using DZO_IntegracijaUstanovaDokumentacija_API.AbstractClasses;
using DZO_IntegracijaUstanovaDokumentacija_API.Interfaces;
using Microsoft.Extensions.Options;


namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public class MediGroupSftpService:BaseSftpService
    {
        private CorisSftpService _sftpService;

        public MediGroupSftpService(IOptions<MediGroupSftpSettings> sftpSettings,CorisSftpService sftpService)
         : base(sftpSettings.Value.Host, sftpSettings.Value.Port, sftpSettings.Value.Username, sftpSettings.Value.Password, sftpSettings.Value.RemotePath)
        {
            _sftpService = sftpService;
        }

        public List<string> ListaZipFajlova()
        {
            Connect();

            var files = new List<string>();

            var directory = _sftpClient.ListDirectory(_remotePath);
            foreach (var fileInfo in directory)
            {
                if (!fileInfo.IsDirectory && fileInfo.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(fileInfo.Name);
                }
            }

            Disconnect();

            return files;
        }

        public List<string> PrebaciZipSFTP(string naziv)
        {
            var remotePath = _remotePath;
            var homePath = $"{remotePath}/{naziv}";
            var destinationPath = $"{"GlobosDokumentacijaMedigroup"}/{naziv}";
            var homePathPOSLATO = $"{remotePath}/POSLATO/";
            var homePathGRESKA = $"{remotePath}/GRESKA/";
            try
            {

                UploadDirectoryContents(remotePath, destinationPath, naziv);

                MoveZipFile(remotePath, homePathPOSLATO, naziv);


                return new List<string> { "Uspeh", "" };
            }   
            catch (Exception ex)
            {
                MoveZipFile(homePath, homePathGRESKA, naziv );

                return new List<string> { "Neuspeh", ex.ToString() };
            }
        }

        private void UploadDirectoryContents(string sourcePath, string destinationPath, string naziv)
        {
            var files = _sftpClient.ListDirectory(sourcePath)
                          .Where(file => file.Name == naziv);
            foreach (var file in files)
            {
                        using (var fileStream = _sftpClient.OpenRead(file.FullName))
                        {
                            _sftpService._sftpClient.UploadFile(fileStream, destinationPath);
                        }
                    
                
            }

        }

        private void MoveZipFile(string sourcePath, string destinationPath, string naziv)
        {
            var files = _sftpClient.ListDirectory(sourcePath)
                          .Where(file => file.Name == naziv);
            foreach (var file in files)
            {
                    file.MoveTo(destinationPath + file.Name);
                
            }
        }
    }
}
