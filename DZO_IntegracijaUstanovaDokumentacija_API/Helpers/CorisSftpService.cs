using DZO_IntegracijaUstanovaDokumentacija_API.AbstractClasses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System.IO;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{

    public class CorisSftpService : BaseSftpService
    {
        private readonly GlobosSftpService _globos;

        public CorisSftpService(IOptions<CorisSftpSetting> sftpSettings, GlobosSftpService globosSftpService)
            : base(sftpSettings.Value.Host, sftpSettings.Value.Port, sftpSettings.Value.Username, sftpSettings.Value.Password, sftpSettings.Value.RemotePath)
        {
            _globos = globosSftpService;
        }

        public SftpTransferResult PrebaciFolderNaCoris(string naziv)
        {
            var sourceRoot = _globos._remotePath; // Globos
            var destRoot = _remotePath;           // CORIS

            var homePath = $"{sourceRoot}/{naziv}";
            var destinationPath = $"{destRoot}/{naziv}";
            var homePathPOSLATO = $"{sourceRoot}/POSLATO/{naziv}";
            var homePathGRESKA = $"{sourceRoot}/GRESKA/{naziv}";

            using (_globos.ConnectScope())
            using (this.ConnectScope())
            {
                try
                {
                    CreateDirectoryRecursively(destinationPath, _sftpClient);
                    UploadDirectoryContents(homePath, destinationPath);
                    MoveFolder(homePath, homePathPOSLATO);
                    return SftpTransferResult.Ok();
                }
                catch (Exception ex)
                {
                    try { MoveFolder(homePath, homePathGRESKA); } catch { }
                    return SftpTransferResult.Fail(ex.Message ?? "Nepoznata greška pri prebacivanju foldera");
                }
            }
        }

        private void UploadDirectoryContents(string sourcePath, string destinationPath)
        {
            var files = _globos._sftpClient.ListDirectory(sourcePath);
            foreach (var file in files)
            {
                if (file.IsDirectory || file.Name.StartsWith(".")) continue;

                using var fileStream = _globos._sftpClient.OpenRead(file.FullName);
                var remoteFilePath = $"{destinationPath}/{file.Name}";
                _sftpClient.UploadFile(fileStream, remoteFilePath);
            }
        }

        private void CreateDirectoryRecursively(string targetPath, SftpClient client)
        {
            if (string.IsNullOrWhiteSpace(targetPath)) return;

            var normalized = targetPath.Replace('\\', '/');
            var startsWithDot = normalized.StartsWith(".");
            var initial = startsWithDot ? "." : "";
            var tail = startsWithDot ? normalized.Substring(1) : normalized;

            tail.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(initial, (current, seg) =>
                {
                    var next = string.IsNullOrEmpty(current) || current == "."
                        ? $"{current}/{seg}".Replace("//", "/")
                        : $"{current}/{seg}";

                    if (!client.Exists(next)) client.CreateDirectory(next);
                    return next;
                });
        }

        private void MoveFolder(string sourcePath, string destinationPath)
        {
            var src = _globos._sftpClient;

            if (!src.Exists(destinationPath))
                CreateDirectoryRecursively(destinationPath, src);

            src.ListDirectory(sourcePath)
               .Where(e => e.Name.Length == 0 || e.Name[0] != '.')
               .ToList()
               .ForEach(entry =>
               {
                   var sourceFilePath = entry.FullName;
                   var destFilePath = $"{destinationPath}/{entry.Name}";

                   if (entry.IsDirectory)
                       MoveFolder(sourceFilePath, destFilePath);
                   else
                       src.RenameFile(sourceFilePath, destFilePath);
               });

            src.DeleteDirectory(sourcePath);
        }
    }
}
