namespace DZO_IntegracijaUstanovaDokumentacija_API.Models.DataTableObjects
{
    public class FajloviTable
    {
        public int? IdSpecifikacije {  get; set; }
        public string? NazivFajla { get; set; }
        public int? StatusId { get; set; }
        public DateTime? DatumPrebacivanja { get; set; }
    }
}
