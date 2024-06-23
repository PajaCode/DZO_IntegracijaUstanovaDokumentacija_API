using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System.IO;
using System.Net.Mail;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public class CorisSftpService
    {
        private readonly CorisSftpSetting _sftpSettingsCor;
        private readonly GlobosSftpService _sftpService;
        private readonly SftpClient _sftpClientCor;
        public string RemotePath => _sftpSettingsCor.RemotePath;
        public CorisSftpService(IOptions<CorisSftpSetting> sftpSettings, GlobosSftpService sftpService)
        {
            _sftpSettingsCor = sftpSettings.Value;
            _sftpClientCor = new SftpClient(_sftpSettingsCor.Host, _sftpSettingsCor.Username, _sftpSettingsCor.Password);
            _sftpService = sftpService;   
        }
        public void Connect()
        {
            if (!_sftpClientCor.IsConnected)
            {
                _sftpClientCor.Connect();
            }
        }

        public void Disconnect()
        {
            if (_sftpClientCor.IsConnected)
            {
                _sftpClientCor.Disconnect();
            }
        }

        public string PrebaciFoldereSFTP(string naziv) {

            _sftpService.Connect();

            Connect();

            var folders = _sftpService.ListaFoldera();

            foreach (var folder in folders)
            {
                try
                {
                    if (folder.Equals(naziv, StringComparison.OrdinalIgnoreCase))
                    {

                        using (var sourceStream = _sftpService._sftpClient.OpenRead(folder))
                        using (var destinationStream = _sftpClientCor.Create($"{_sftpSettingsCor.RemotePath}/{folder}"))
                        {
                            sourceStream.CopyTo(destinationStream);
                        }

                        continue;
                    }
                    
                   

                }
                catch (Exception ex)
                {

                    return ex.ToString();
                }
               
            }

            return "kraj";



        }
       



    }

}

