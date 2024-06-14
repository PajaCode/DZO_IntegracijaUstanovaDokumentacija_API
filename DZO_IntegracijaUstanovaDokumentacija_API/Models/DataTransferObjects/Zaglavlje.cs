namespace DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects
{
    public class Zaglavlje
    {
        public string FakturaId { get; set; }
        public string FakturaBroj { get; set; }
        public int UstanovaIDMG { get; set; }
        public DateTime Datum { get; set; }
    }
}
