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
        public void PrebaciFajlove(string brUputa, int IdSpec , string NazivFajla)
        {
           

           
                try
                {
                    string putanjaDoFajla = Path.Combine(_sftpSettings.RemotePath, NazivFajla);
                    string putanjaDoFoldera = Path.Combine(_sftpSettings.RemotePath, brUputa);

                    // Proveravamo da li fajl postoji pre nego što ga premestimo
                    if (File.Exists(putanjaDoFajla))
                    {
                        // Formiramo putanju za novu lokaciju fajla
                        string novaPutanjaDoFajla = Path.Combine(putanjaDoFoldera, Path.GetFileName(putanjaDoFajla));

                        // Premestamo fajl
                        File.Move(putanjaDoFajla, novaPutanjaDoFajla);
                        //Console.WriteLine($"Fajl '{imeFajla}' uspešno premesten u folder '{putanjaDoFoldera}'.");
                    }
                    else
                    {
                        //Console.WriteLine($"Fajl '{imeFajla}' ne postoji na zadatoj putanji.");
                    }
                }
                catch (Exception ex)
                {
                   // Console.WriteLine($"Greška prilikom premestanja fajla '{imeFajla}': {ex.Message}");
                }
            
        }



    }

}

