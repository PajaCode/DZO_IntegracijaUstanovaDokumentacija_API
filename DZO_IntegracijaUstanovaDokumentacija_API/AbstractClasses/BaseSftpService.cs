using DZO_IntegracijaUstanovaDokumentacija_API.Interfaces;


namespace DZO_IntegracijaUstanovaDokumentacija_API.AbstractClasses
{
    public abstract class BaseSftpService : ISftpService
    {
        public SftpClient _sftpClient;
        public string _remotePath;

        public BaseSftpService(string host, string username, string password, string remotePath)
        {
            _sftpClient = new SftpClient(host, username, password);
            _remotePath = remotePath;
        }

        public virtual void Connect()
        {
            if (!_sftpClient.IsConnected)
            {
                _sftpClient.Connect();
            }
        }

        public virtual void Disconnect()
        {
            if (_sftpClient.IsConnected)
            {
                _sftpClient.Disconnect();
            }
        }
    }

}
