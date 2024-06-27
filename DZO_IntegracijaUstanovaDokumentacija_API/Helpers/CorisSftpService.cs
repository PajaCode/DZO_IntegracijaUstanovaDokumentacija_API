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
        
        private readonly GlobosSftpService _sftpService;
       
        public CorisSftpService(IOptions<CorisSftpSetting> sftpSettings, GlobosSftpService sftpService)
            :base(sftpSettings.Value.Host, sftpSettings.Value.Username, sftpSettings.Value.Password, sftpSettings.Value.RemotePath)
        {
            
            _sftpService = sftpService;   
        }
     

        public List<string> PrebaciFoldereSFTP(string naziv, string folder)
        {
            try
            {
                _sftpService._sftpClient.Connect();

                var remotePath = _sftpSettingsCor.RemotePath;
                var homePath = _sftpService.RemotePath;
                var destinationPath = $"{remotePath}/{naziv}";

                CreateDirectoryRecursively(destinationPath, _sftpService._sftpClient);

                UploadDirectoryContents(homePath, destinationPath, _sftpService._sftpClient);

                return new List<string> { "uspeh", "" }; // Success
            }
            catch (Exception ex)
            {
                return new List<string> { "neuspeh", ex.ToString() }; // Failure
            }
        }

        private void UploadDirectoryContents(string sourcePath, string destinationPath, SftpClient client)
        {
            var files = client.ListDirectory(sourcePath);
            foreach (var file in files)
            {
                if (!file.IsDirectory)
                {
                    using (var fileStream = client.OpenRead(file.FullName))
                    {
                        var remoteFilePath = $"{destinationPath}/{file.Name}";
                        client.UploadFile(fileStream, remoteFilePath);
                    }
                }
            }

            //foreach (var directory in files.Where(f => f.IsDirectory))
            //{
            //    var remoteDirectoryPath = $"{destinationPath}/{directory.Name}";

            //    CreateDirectoryRecursively(remoteDirectoryPath, client);
            //    UploadDirectoryContents(directory.FullName, remoteDirectoryPath, client);
            //}
        }

        private void CreateDirectoryRecursively(string targetPath, SftpClient client)
        {
            string currentPath = "";
            if (targetPath[0] == '.')
            {
                currentPath = ".";
                targetPath = targetPath[1..];
            }
            foreach (string segment in targetPath.Split('/'))
            {
                // Ignoring leading/ending/multiple slashes
                if (!string.IsNullOrWhiteSpace(segment))
                {
                    currentPath += $"/{segment}";
                    if (!client.Exists(currentPath))
                        client.CreateDirectory(currentPath);
                }
            }
        }



    }

}

