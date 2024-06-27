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
     

        public  List<string> PrebaciFoldereSFTP(string naziv, string folder) {

            try
            {
                using (var sourceStream = _sftpService._sftpClient.OpenRead(folder))
                using (var destinationStream = _sftpClient.Create($"{_remotePath}/{folder}"))
                {
                    sourceStream.CopyTo(destinationStream);
                    //kopiraj u prebaceno
                }

                
                return ["uspeh",""];

            }
            catch (Exception ex)
            {     
                
                return ["neuspeh", ex.ToString()];
                
            }
                     
                    
                          
               
            

       



        }
       



    }

}

