namespace DZO_IntegracijaUstanovaDokumentacija_API.Errors
{
    public sealed record ItemError(
        int? IdJson,
        int? IdSpec,
        string? FileName,
        string Message
    );
}
