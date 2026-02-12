using DZO_IntegracijaUstanovaDokumentacija_API.Interfaces;
using System.Text;


namespace DZO_IntegracijaUstanovaDokumentacija_API.AbstractClasses
{
    public abstract class BaseSftpService : ISftpService
    {
        public SftpClient _sftpClient;
        public string _remotePath;

        protected System.TimeSpan _operationTimeout = System.TimeSpan.FromSeconds(60);
        protected System.TimeSpan _keepAlive = System.TimeSpan.FromSeconds(30);
        protected uint _bufferSize = 128 * 1024;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private bool _disposed;

        public BaseSftpService(string host, int port, string username, string password, string remotePath)
        {
            _sftpClient = new SftpClient(host, port, username, password);
            _remotePath = NormalizeRemotePath(remotePath);

            _sftpClient.OperationTimeout = _operationTimeout;
            _sftpClient.KeepAliveInterval = _keepAlive;
            _sftpClient.BufferSize = _bufferSize;
        }

        public bool IsConnected => _sftpClient?.IsConnected == true;

        public virtual void Connect()
        {
            _gate.Wait();
            try
            {
                ThrowIfDisposed();
                if (!_sftpClient.IsConnected)
                {
                    _sftpClient.Connect();
                    EnsureDirectoryExists(_remotePath);
                }
            }
            finally { _gate.Release(); }
        }

        public virtual void Disconnect()
        {
            _gate.Wait();
            try
            {
                if (_sftpClient.IsConnected) _sftpClient.Disconnect();
            }
            finally { _gate.Release(); }
        }

        public System.IDisposable ConnectScope()
        {
            var opened = false;
            _gate.Wait();
            try
            {
                ThrowIfDisposed();
                if (!_sftpClient.IsConnected)
                {
                    _sftpClient.Connect();
                    EnsureDirectoryExists(_remotePath);
                    opened = true;
                }
            }
            finally { _gate.Release(); }

            return new Scope(this, opened);
        }

        private readonly struct Scope : System.IDisposable
        {
            private readonly BaseSftpService _svc;
            private readonly bool _shouldClose;
            public Scope(BaseSftpService svc, bool shouldClose) { _svc = svc; _shouldClose = shouldClose; }
            public void Dispose()
            {
                if (_shouldClose) _svc.Disconnect();
            }
        }

        protected static string NormalizeRemotePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "/";
            var p = path.Replace('\\', '/').Trim();
            if (!p.StartsWith("/")) p = "/" + p;
            while (p.Contains("//")) p = p.Replace("//", "/");
            return p.TrimEnd('/');
        }

        protected string CombineRemote(params string[] parts)
        {
            var sb = new StringBuilder(_remotePath);
            foreach (var part in parts)
            {
                var seg = (part ?? string.Empty).Replace('\\', '/').Trim('/');
                if (seg.Length == 0) continue;
                sb.Append('/').Append(seg);
            }
            return sb.ToString();
        }

        protected void EnsureDirectoryExists(string targetPath)
        {
            var client = _sftpClient;
            var path = NormalizeRemotePath(targetPath);
            if (client.Exists(path)) return;

            string current = "";
            foreach (var segment in path.Split('/', System.StringSplitOptions.RemoveEmptyEntries))
            {
                current += "/" + segment;
                if (!client.Exists(current)) client.CreateDirectory(current);
            }
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed) throw new System.ObjectDisposedException(GetType().Name);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { Disconnect(); } catch { }
            _gate.Dispose();
            System.GC.SuppressFinalize(this);
        }
    }

}
