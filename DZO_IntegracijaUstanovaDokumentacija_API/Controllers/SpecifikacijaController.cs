
using DZO_IntegracijaUstanovaDokumentacija_API.Api;
using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using HR_API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Renci.SshNet.Messages;
using static System.Net.WebRequestMethods;


namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecifikacijaController : ControllerBase
    {
        private readonly SpecifikacijaManager _manager;

        public SpecifikacijaController(SpecifikacijaManager manager)
        {
            _manager = manager;
        }

        [HttpGet("upisiJsone")]
        public ActionResult<ApiResponse<OperationSummary>> UpisiJsone()
        {
            var summary = _manager.UpisiJsone();
            return Ok(ApiResponse<OperationSummary>.FromOperation(HttpContext, summary));
        }

        [HttpGet("parsirajIinsertuj")]
        public async Task<ActionResult<ApiResponse<OperationSummary>>> ParsirajIinsertuj()
        {
            var summary = await _manager.ParsirajIInsertujAsync();

            if (summary.Status == ApiExecutionStatus.Failed)
                return StatusCode(500, ApiResponse<OperationSummary>.Fail(HttpContext, "OPERATION_FAILED", summary.Message ?? "Neuspeh", summary));

            return Ok(ApiResponse<OperationSummary>.FromOperation(HttpContext, summary));
        }
    }
}
