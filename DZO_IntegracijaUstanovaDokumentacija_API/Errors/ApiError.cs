namespace DZO_IntegracijaUstanovaDokumentacija_API.Errors
{
    public sealed record ApiError(
        string Code,
        string Message
    );
}
