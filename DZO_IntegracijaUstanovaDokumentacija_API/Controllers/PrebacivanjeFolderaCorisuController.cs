using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrebacivanjeFolderaCorisuController : ControllerBase
    {
        private readonly GlobosSftpService _sftpService;
        private readonly CorisSftpService _sftpServiceCor;
        private readonly RazmenaDokumentacijeDb_Context _db;
        private readonly Logovi _logger;
        public PrebacivanjeFolderaCorisuController(GlobosSftpService sftpService, CorisSftpService sftpServiceCor, RazmenaDokumentacijeDb_Context db, Logovi logger)
        {
            _sftpService = sftpService;
            _sftpServiceCor = sftpServiceCor;
            _db = db;
            _logger = logger;   
        }
        [HttpGet("prebaciFoldere")]
        public IActionResult PrebaciFoldere()
        {
            try
            {
                PrebacivanjeFolderaCorisuManager manager = new(_sftpService,_sftpServiceCor,_db, _logger);
                manager.PrebaciFoldere();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"prebacivanje foldera: {ex.Message}");
            }

        }
    }
}
