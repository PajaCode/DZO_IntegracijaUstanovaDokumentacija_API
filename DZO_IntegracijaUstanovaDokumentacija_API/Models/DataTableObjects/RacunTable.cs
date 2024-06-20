namespace DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTableObjects
{
    public class RacunTable
    {
        public int? IdSpecifikacije {  get; set; }
        public string RacunID { get; set; }
        public DateTime RacunDatum { get; set; }
        public string RacunBrojFiskala { get; set; }
        public string UputBroj { get; set; }
        public string BrojKartice { get; set; }
        public decimal UkupanIznos { get; set; }
        public decimal? Participacija { get; set; }
        public decimal? Popust { get; set; }
        public string RacunFajl { get; set; }
        public string UputFajl { get; set; }
    }
}
