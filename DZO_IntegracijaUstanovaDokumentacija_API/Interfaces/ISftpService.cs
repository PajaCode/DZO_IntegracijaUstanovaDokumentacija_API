namespace DZO_IntegracijaUstanovaDokumentacija_API.Interfaces
{

    public interface ISftpService : System.IDisposable
    {
        void Connect();
        void Disconnect();
        bool IsConnected { get; }
    }

}
