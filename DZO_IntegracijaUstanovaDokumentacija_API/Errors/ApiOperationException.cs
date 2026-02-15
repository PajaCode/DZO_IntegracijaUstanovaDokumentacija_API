namespace DZO_IntegracijaUstanovaDokumentacija_API.Errors
{
    public sealed class ApiOperationException : Exception
    {
        public int HttpStatus { get; }
        public string Code { get; }

        public int? IdJson { get; }
        public int? IdSpec { get; }
        public int? IdMetoda { get; }

        public ApiOperationException(
            string code,
            string message,
            int httpStatus = 500,
            int? idJson = null,
            int? idSpec = null,
            int? idMetoda = null,
            Exception? inner = null)
            : base(message, inner)
        {
            Code = code;
            HttpStatus = httpStatus;
            IdJson = idJson;
            IdSpec = idSpec;
            IdMetoda = idMetoda;
        }
    }
}
