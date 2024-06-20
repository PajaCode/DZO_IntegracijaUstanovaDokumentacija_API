namespace DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects
{
    public class Stavka
    {
        public int StavkaID { get; set; }
        public DateTime UslugaDatum {  get; set; }
        public decimal Popust { get; set; }
        public string Valuta { get; set; }
        public decimal PunaCena { get; set; }
        public decimal ZaUplatu { get; set; }
        public string UslugaNaziv { get; set; }
        public int SARADNIKID { get; set; }
        public string SARADNIKNAZIV { get; set; }
        public int UslugaID { get; set; }
        public string NalazFajl { get; set; }
        public string LabNalazFajl { get; set; }
        public string Nalazsistematski { get; set; }

        public string Attachments { get; set; }
    }
}
