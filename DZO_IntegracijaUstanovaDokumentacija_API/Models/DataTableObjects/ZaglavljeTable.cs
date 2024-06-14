namespace DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTableObjects
{
    public class ZaglavljeTable
    {
        public string FakturaId { get; set; }
        public string FakturaBroj { get; set; }
        public int UstanovaIDMG { get; set; }
        public DateTime Datum { get; set; }
        public string OpisJson {  get; set; }
        public int? StatusId { get; set; }
    }
}
