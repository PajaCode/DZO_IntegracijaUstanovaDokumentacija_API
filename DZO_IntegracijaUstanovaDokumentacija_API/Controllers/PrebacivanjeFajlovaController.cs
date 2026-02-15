using DZO_IntegracijaUstanovaDokumentacija_API.Api;
using DZO_IntegracijaUstanovaDokumentacija_API.Errors;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrebacivanjeFajlovaController : ControllerBase
    {
        private readonly PrebacivanjeFajlovaManager _manager;

        public PrebacivanjeFajlovaController(PrebacivanjeFajlovaManager manager)
        {
            _manager = manager;
        }

        [HttpGet("kreirajFoldere")]
        public ActionResult<ApiResponse<OperationSummary>> KreirajFoldere()
        {
            var summary = _manager.KreirajFoldere();
            return Ok(ApiResponse<OperationSummary>.FromOperation(HttpContext, summary));
        }

        [HttpGet("prebaciFajlove")]
        public async Task<ActionResult<ApiResponse<OperationSummary>>> PrebaciFajlove()
        {
            var summary = await _manager.PrebaciFajloveAsync();
            return Ok(ApiResponse<OperationSummary>.FromOperation(HttpContext, summary));
        }
    }
}
