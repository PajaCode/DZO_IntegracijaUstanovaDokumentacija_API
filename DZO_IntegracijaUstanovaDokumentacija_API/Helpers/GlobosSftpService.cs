using DZO_IntegracijaUstanovaDokumentacija_API.AbstractClasses;
using Microsoft.Extensions.Options;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public class GlobosSftpService : BaseSftpService
    {
        public GlobosSftpService(IOptions<GlobosSftpSetting> sftpSettings)
            : base(sftpSettings.Value.Host, sftpSettings.Value.Port, sftpSettings.Value.Username, sftpSettings.Value.Password, sftpSettings.Value.RemotePath)
        { }

        public List<string> ListaJsona()
        {
            using (ConnectScope())
            {
                return _sftpClient.ListDirectory(_remotePath)
                    .Where(f => !f.IsDirectory
                             && f.Name.Length >= 6                   // .json
                             && f.Name[0] != '.'
                             && f.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    .Select(f => f.Name)
                    .ToList();
            }
        }

        public string UzmiSadrzajFajla(string file)
        {
            using (ConnectScope())
            using (var stream = _sftpClient.OpenRead(file))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public string KreirajFolderNaSFTP(string nazivFoldera)
        {
            using (ConnectScope())
            {
                var full = $"{_remotePath}/{nazivFoldera}";
                if (!_sftpClient.Exists(full))
                {
                    var segments = full.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
                    var current = "";
                    foreach (var seg in segments)
                    {
                        current += "/" + seg;
                        if (!_sftpClient.Exists(current)) _sftpClient.CreateDirectory(current);
                    }
                    return "Uspeh";
                }
                else
                {
                    return "Neuspeh";
                }
            }
        }

        public bool PrebaciFajlove(string brUputa, string NazivFajla)
        {
            var putanjaDoFajla = $"{_remotePath}/{NazivFajla}";
            var putanjaDoFoldera = $"{_remotePath}/{brUputa}";

            using (ConnectScope())
            {
                if (_sftpClient.Exists(putanjaDoFoldera) && _sftpClient.Exists(putanjaDoFajla))
                {
                    _sftpClient.RenameFile(putanjaDoFajla, $"{putanjaDoFoldera}/{NazivFajla}");
                    return true;
                }
                return false;
            }
        }

        public List<string> ListaFoldera()
        {
            using (ConnectScope())
            {
                return _sftpClient.ListDirectory(_remotePath)
                                  .Where(f => f.IsDirectory && !f.Name.StartsWith("."))
                                  .Select(f => f.Name)
                                  .ToList();
            }
        }

        public string VratiPutanju() => _remotePath;

        public void PostojiFajl(List<string> files, string jsonFile)
        {
            using (ConnectScope())
            {
                foreach (var f in files)
                {
                    if (!_sftpClient.Exists($"{_remotePath}/{f}"))
                    {
                        var filePath = $"{_remotePath}/{jsonFile}";
                        var errorRoot = $"{_remotePath}/GRESKA";
                        var errorPath = $"{errorRoot}/{jsonFile}";

                        if (!_sftpClient.Exists(errorRoot))
                        {
                            var segments = errorRoot.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
                            var current = "";
                            foreach (var seg in segments)
                            {
                                current += "/" + seg;
                                if (!_sftpClient.Exists(current)) _sftpClient.CreateDirectory(current);
                            }
                        }

                        _sftpClient.RenameFile(filePath, errorPath);
                        throw new System.Exception($"{f} ne postoji na SFTP serveru");
                    }
                }
            }
        }
    }
}