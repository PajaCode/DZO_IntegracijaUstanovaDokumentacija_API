namespace DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTransferObjects
{
    public class Racun
    {
        public string RacunID { get; set; }
        public DateTime RacunDatum { get; set; }
        public string RacunBrojFiskala { get; set; }
        public string UputBroj { get; set; }
        public string PacijentID { get; set; }
        public decimal IZNOSCLAIM { get; set; }
        public decimal Participacija {  get; set; }
        public decimal Popust {  get; set; }
        public List<Stavka> Stavka { get; set; }
        public string RacunFajl { get; set; }
        public string UputFajl { get; set; }
    }
}
