namespace DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects
{
    public class GlobosSftpSetting
    {
       
            public string Host { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string RemotePath { get; set; }
            public int Port {  get; set; }
    }
}
