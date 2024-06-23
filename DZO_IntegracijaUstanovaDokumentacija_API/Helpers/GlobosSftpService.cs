using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System.IO;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public class GlobosSftpService
    {
        private readonly GlobosSftpSetting _sftpSettings;
        private SftpClient _sftpClient;
        public string RemotePath => _sftpSettings.RemotePath;
        public GlobosSftpService(IOptions<GlobosSftpSetting> sftpSettings)
        {
            _sftpSettings = sftpSettings.Value;
            _sftpClient = new SftpClient(_sftpSettings.Host, _sftpSettings.Username, _sftpSettings.Password);
        }
        public void Connect()
        {
            if (!_sftpClient.IsConnected)
            {
                _sftpClient.Connect();
            }
        }

        public void Disconnect()
        {
            if (_sftpClient.IsConnected)
            {
                _sftpClient.Disconnect();
            }
        }

        public List<string> ListaJsona()
        {
            Connect();

            var files = new List<string>();

            var directory = _sftpClient.ListDirectory(_sftpSettings.RemotePath);
            foreach (var fileInfo in directory)
            {
                if (!fileInfo.IsDirectory && fileInfo.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(fileInfo.Name);
                }
            }

            

            return files;
        }

        public string UzmiSadrzajFajla(string file)
        {
            Connect();
            using (var stream = _sftpClient.OpenRead(file))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }

            }

           
        }

        public string  KreirajFolderNaSFTP(string nazivFoldera, int idJson)
        {
           
              Connect();

            string putanjaDoFoldera = Path.Combine(_sftpSettings.RemotePath, nazivFoldera);

            if (!_sftpClient.Exists(putanjaDoFoldera))
            {
                _sftpClient.CreateDirectory(putanjaDoFoldera);
                return "Uspeh";
            }
            else
            {
                return "Neuspeh";
            }


        }
        public string PrebaciFajlove(string brUputa , string NazivFajla)
        {
          
             string putanjaDoFajla = Path.Combine(_sftpSettings.RemotePath, NazivFajla);
             string putanjaDoFoldera = Path.Combine(_sftpSettings.RemotePath, brUputa);
             
             Connect();

                var files = _sftpClient.ListDirectory(_sftpSettings.RemotePath);

                bool pdfExists = files.Any(f => f.Name == NazivFajla && !f.IsDirectory);

                if (pdfExists)
                {
                    _sftpClient.RenameFile(_sftpSettings.RemotePath, putanjaDoFoldera + pdfExists);
                    return "Uspeh";
                }
                else
                {
                    return "Nespeh";
                }
            
              
            
        }



    }

}

