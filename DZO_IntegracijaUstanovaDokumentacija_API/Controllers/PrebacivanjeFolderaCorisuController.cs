using DZO_IntegracijaUstanovaDokumentacija_API.Api;
using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrebacivanjeFolderaCorisuController : ControllerBase
    {
        private readonly PrebacivanjeFolderaCorisuManager _manager;

        public PrebacivanjeFolderaCorisuController(PrebacivanjeFolderaCorisuManager manager)
        {
            _manager = manager;
        }

        [HttpGet("prebaciFoldere")]
        public ActionResult<ApiResponse<OperationSummary>> PrebaciFoldere()
        {
            var summary = _manager.PrebaciFoldere();
            return Ok(ApiResponse<OperationSummary>.FromOperation(HttpContext, summary));
        }
    }
}
