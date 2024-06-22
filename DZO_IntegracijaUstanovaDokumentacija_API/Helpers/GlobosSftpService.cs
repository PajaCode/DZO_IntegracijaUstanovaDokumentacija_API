using Microsoft.Extensions.Options;
using Renci.SshNet;
using System.IO;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Helpers
{
    public class GlobosSftpService : IDisposable
    {
        private readonly GlobosSftpSetting _sftpSettings;
        private SftpClient _sftpClient;

        public GlobosSftpService(IOptions<GlobosSftpSetting> sftpSettings)
        {
            _sftpSettings = sftpSettings.Value;
            _sftpClient = new SftpClient(_sftpSettings.Host, _sftpSettings.Username, _sftpSettings.Password);
        }
        public void Connect()
        {
            if (!_sftpClient.IsConnected)
            {
                _sftpClient.Connect();
            }
        }

        public void Disconnect()
        {
            if (_sftpClient.IsConnected)
            {
                _sftpClient.Disconnect();
            }
        }

        public IEnumerable<string> ListaJsona()
        {
            Connect();

            var files = new List<string>();

            var directory = _sftpClient.ListDirectory(_sftpSettings.RemotePath);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Oslobađanje managed resursa
                if (_sftpClient != null)
                {
                    _sftpClient.Dispose();
                    _sftpClient = null;
                }
            }
            // Oslobađanje unmanaged resursa
        }
    }

}

