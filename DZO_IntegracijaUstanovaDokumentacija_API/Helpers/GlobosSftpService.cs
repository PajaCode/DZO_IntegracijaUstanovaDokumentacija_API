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
                             && f.Name.Length >= 6
                             && f.Name[0] != '.'
                             && f.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    .Select(f => f.Name)
                    .ToList();
            }
        }

        private string ResolvePath(string fileOrPath)
        {
            if (string.IsNullOrWhiteSpace(fileOrPath)) return _remotePath;
            var p = fileOrPath.Replace('\\', '/').Trim();
            return p.Contains('/') ? p : $"{_remotePath}/{p}";
        }

        public string UzmiSadrzajFajla(string fileName)
        {
            using (ConnectScope())
            {
                var fullPath = ResolvePath(fileName);
                using var stream = _sftpClient.OpenRead(fullPath);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public SftpFolderCreateResult KreirajFolderNaSftp(string nazivFoldera)
        {
            using (ConnectScope())
            {
                var full = $"{_remotePath}/{nazivFoldera}";
                if (_sftpClient.Exists(full))
                    return new SftpFolderCreateResult(SftpFolderCreateOutcome.AlreadyExists);

                var segments = full.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var current = "";
                foreach (var seg in segments)
                {
                    current += "/" + seg;
                    if (!_sftpClient.Exists(current)) _sftpClient.CreateDirectory(current);
                }

                return new SftpFolderCreateResult(SftpFolderCreateOutcome.Created);
            }
        }

        public SftpMoveResult PrebaciFajlUFolder(string brUputa, string nazivFajla)
        {
            var putanjaDoFajla = $"{_remotePath}/{nazivFajla}";
            var putanjaDoFoldera = $"{_remotePath}/{brUputa}";

            using (ConnectScope())
            {
                if (!_sftpClient.Exists(putanjaDoFoldera))
                    return SftpMoveResult.Fail("FOLDER_NOT_FOUND", $"Folder ne postoji: {putanjaDoFoldera}");

                if (!_sftpClient.Exists(putanjaDoFajla))
                    return SftpMoveResult.Fail("FILE_NOT_FOUND", $"Fajl ne postoji: {putanjaDoFajla}");

                _sftpClient.RenameFile(putanjaDoFajla, $"{putanjaDoFoldera}/{nazivFajla}");
                return SftpMoveResult.Ok();
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
                    if (_sftpClient.Exists($"{_remotePath}/{f}")) continue;

                    var filePath = $"{_remotePath}/{jsonFile}";
                    var errorRoot = $"{_remotePath}/GRESKA";
                    var errorPath = $"{errorRoot}/{jsonFile}";

                    if (!_sftpClient.Exists(errorRoot))
                    {
                        var segments = errorRoot.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        var current = "";
                        foreach (var seg in segments)
                        {
                            current += "/" + seg;
                            if (!_sftpClient.Exists(current)) _sftpClient.CreateDirectory(current);
                        }
                    }

                    _sftpClient.RenameFile(filePath, errorPath);
                    throw new Exception($"{f} ne postoji na SFTP serveru");
                }
            }
        }
    }
}