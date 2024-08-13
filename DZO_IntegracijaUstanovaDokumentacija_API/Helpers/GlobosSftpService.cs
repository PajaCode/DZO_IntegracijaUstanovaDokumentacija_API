using DZO_IntegracijaUstanovaDokumentacija_API.AbstractClasses;
using DZO_IntegracijaUstanovaDokumentacija_API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Net.WebRequestMethods;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public class GlobosSftpService : BaseSftpService
    {

        public GlobosSftpService(IOptions<GlobosSftpSetting> sftpSettings)
         : base(sftpSettings.Value.Host, sftpSettings.Value.Port, sftpSettings.Value.Username, sftpSettings.Value.Password, sftpSettings.Value.RemotePath)
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

            Disconnect();

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

            // string putanjaDoFoldera = _remotePath+" / "+nazivFoldera;

            if (!_sftpClient.Exists(nazivFoldera))
            {
                _sftpClient.CreateDirectory(nazivFoldera);


                return "Uspeh";
            }
            else
            {
                return "Neuspeh";
            }


        }
        public bool PrebaciFajlove(string brUputa, string NazivFajla)
        {

            string putanjaDoFajla = _remotePath + "/" + NazivFajla;
            string putanjaDoFoldera = _remotePath + "/" + brUputa;

            Connect();

            var files = _sftpClient.ListDirectory(_remotePath);

            bool pdfExists = files.Any(f => f.Name == NazivFajla && !f.IsDirectory);

            if (_sftpClient.Exists(putanjaDoFoldera))
            {
                if (pdfExists)
                {
                    _sftpClient.RenameFile(putanjaDoFajla, putanjaDoFoldera + "/" + NazivFajla);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
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


        public string VratiPutanju()
        {
            return _remotePath;
        }


        public void PostojiFajl(List<string> files, string jsonFile)
        {
            Connect();

            foreach (string file in files)
            {
                string filePath = $"{_remotePath}/{jsonFile}";
                string errorPath = $"{_remotePath}/GRESKA/{jsonFile}";
                if (!_sftpClient.Exists($"{_remotePath}/{file}"))
                {
                    _sftpClient.RenameFile(filePath, errorPath);
                    throw new Exception($"{file} ne postoji na SFTP serveru");
                }

            }
            Disconnect();
        }

    }

}

