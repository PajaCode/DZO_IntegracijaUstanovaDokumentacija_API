using DZO_IntegracijaUstanovaDokumentacija_API.Api;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using Microsoft.AspNetCore.Diagnostics;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Errors
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // default mapping
            var status = StatusCodes.Status500InternalServerError;
            var code = "UNHANDLED_ERROR";
            var message = _env.IsDevelopment() ? exception.ToString() : "Desila se neočekivana greška.";

            // domain mapping
            if (exception is ApiOperationException opEx)
            {
                status = opEx.HttpStatus;
                code = opEx.Code;
                message = _env.IsDevelopment() ? $"{opEx.Message}\n{opEx}" : opEx.Message;

                // ako imamo dovoljno konteksta, upiši i u tvoje Error tabele
                try
                {
                    if (opEx.IdJson.HasValue && opEx.IdMetoda.HasValue)
                    {
                        var logovi = httpContext.RequestServices.GetService(typeof(Logovi)) as Logovi;
                        if (logovi != null)
                        {
                            logovi.LogError(opEx.IdJson.Value, opEx.Message, opEx.IdSpec ?? 0, opEx.IdMetoda.Value);
                        }
                    }
                }
                catch { /* nikad ne ruši response zbog logovanja */ }
            }

            _logger.LogError(exception, "API error. CorrelationId={CorrelationId}", httpContext.TraceIdentifier);

            httpContext.Response.StatusCode = status;
            httpContext.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(httpContext, code, message);

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
    }
}
