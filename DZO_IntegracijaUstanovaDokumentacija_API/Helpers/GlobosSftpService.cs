using DZO_IntegracijaUstanovaDokumentacija_API.AbstractClasses;
using DZO_IntegracijaUstanovaDokumentacija_API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Collections.Generic;
using System.IO;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public class GlobosSftpService : BaseSftpService
    {
        
        public GlobosSftpService(IOptions<GlobosSftpSetting> sftpSettings)
         : base(sftpSettings.Value.Host, sftpSettings.Value.Username, sftpSettings.Value.Password, sftpSettings.Value.RemotePath)
        {
            
        }



        public List<string> ListaJsona()
        {
            Connect();

            var files = new List<string>();

            var directory = _sftpClient.ListDirectory(_remotePath);
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

        public string KreirajFolderNaSFTP(string nazivFoldera)
        {

            Connect();

            string putanjaDoFoldera = _sftpSettings.RemotePath+"/"+nazivFoldera;

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
        public string PrebaciFajlove(string brUputa, string NazivFajla)
        {

            string putanjaDoFajla = _sftpSettings.RemotePath + "/" + NazivFajla;
            string putanjaDoFoldera = _sftpSettings.RemotePath + "/" + brUputa;

            Connect();

            var files = _sftpClient.ListDirectory(_remotePath);

            bool pdfExists = files.Any(f => f.Name == NazivFajla && !f.IsDirectory);

            if (pdfExists)
            {
                _sftpClient.RenameFile(putanjaDoFajla, putanjaDoFoldera + "/" + NazivFajla);
                return "Uspeh";
            }
            else
            {
                return "Nespeh";
            }



        }

        public List<string> ListaFoldera()
        {
            
            Connect();

            var folderi = _sftpClient.ListDirectory(_remotePath)
                                     .Where(f => f.IsDirectory)
                                     .Select(f => f.Name)
                                     .ToList();

            Disconnect();

            return folderi;


        }

     

    }

}

