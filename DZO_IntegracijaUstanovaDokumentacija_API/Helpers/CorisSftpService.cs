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
            :base(sftpSettings.Value.Host,sftpSettings.Value.Port ,sftpSettings.Value.Username, sftpSettings.Value.Password, sftpSettings.Value.RemotePath)
        {
            
            _sftpService = sftpService;   
        }
     

        public List<string> PrebaciFoldereSFTP(string naziv)
        {
            var remotePath = _remotePath;
            var homePath = $"{_sftpService._remotePath}/{naziv}";
            var destinationPath = $"{remotePath}/{naziv}";
            var homePathPOSLATO = $"{_sftpService._remotePath}/POSLATO/{naziv}";
            var homePathGRESKA = $"{_sftpService._remotePath}/GRESKA/{naziv}";
            try
            {
                _sftpService._sftpClient.Connect();               

                CreateDirectoryRecursively(destinationPath, _sftpClient);

                UploadDirectoryContents(homePath, destinationPath);

                MoveFolder(homePath, homePathPOSLATO);

                return new List<string> { "Uspeh", "" }; 
            }
            catch (Exception ex)
            {
                MoveFolder(homePath, homePathGRESKA);

                return new List<string> { "Neuspeh", ex.ToString() }; 
            }
        }

        private void UploadDirectoryContents(string sourcePath, string destinationPath)
        {
            var files = _sftpService._sftpClient.ListDirectory(sourcePath);
            foreach (var file in files)
            {
                if (!file.IsDirectory)
                {
                    using (var fileStream = _sftpService._sftpClient.OpenRead(file.FullName))
                    {
                        var remoteFilePath = $"{destinationPath}/{file.Name}";
                        _sftpClient.UploadFile(fileStream, remoteFilePath);
                    }
                }
            }

            
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

        private void MoveFolder(string sourcePath, string destinationPath)
        {
            if (!_sftpService._sftpClient.Exists(destinationPath))
            {
                CreateDirectoryRecursively(destinationPath, _sftpService._sftpClient);
            }

            var files = _sftpService._sftpClient.ListDirectory(sourcePath);
            foreach (var file in files)
            {
                if (!file.Name.StartsWith("."))
                {
                    var sourceFilePath = file.FullName;
                    var destFilePath = $"{destinationPath}/{file.Name}";

                    if (file.IsDirectory)
                    {
                        MoveFolder(sourceFilePath, destFilePath);
                    }
                    else
                    {
                        _sftpService._sftpClient.RenameFile(sourceFilePath, destFilePath);
                    }
                }
            }

            _sftpService._sftpClient.DeleteDirectory(sourcePath);
        }

    }

}

