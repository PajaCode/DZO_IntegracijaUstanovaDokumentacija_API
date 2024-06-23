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

        public  List<string> PrebaciFoldereSFTP(string naziv, string folder) {

            try
            {
                using (var sourceStream = _sftpService._sftpClient.OpenRead(folder))
                using (var destinationStream = _sftpClientCor.Create($"{_sftpSettingsCor.RemotePath}/{folder}"))
                {
                    sourceStream.CopyTo(destinationStream);
                    //kopiraj u prebaceno
                }

                
                return ["uspeh",""];

            }
            catch (Exception ex)
            {
                //kopiraj u gresku
                return ["neuspeh", ex.ToString()];
            }
                     
                    
                          
               
            

       



        }
       



    }

}

