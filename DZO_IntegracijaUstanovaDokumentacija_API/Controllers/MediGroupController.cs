using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediGroupController : ControllerBase
    {
        private readonly MediGroupSftpService _sftpService;
        private readonly RazmenaDokumentacijeDb_Context _db;
        private readonly CorisSftpService _sftpServiceCor;
        private readonly Logovi _logger;

        public MediGroupController(MediGroupSftpService sftpService,CorisSftpService sfptServiceCor ,RazmenaDokumentacijeDb_Context db, Logovi logger)
        {
            _sftpService = sftpService;
            _db = db;
            _logger = logger;
            _sftpServiceCor= sfptServiceCor;
        }


        [HttpGet("upisiZip")]
        public IActionResult UpisiZip()
        {
            try
            {
                MediGroupManager manager = new(_sftpService, _sftpServiceCor, _db, _logger);
                manager.UpisiZip();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }

        [HttpGet("prebaciZip")]
        public IActionResult PrebaciZip()
        {
            try
            {
                MediGroupManager manager = new(_sftpService,_sftpServiceCor, _db,_logger);
                manager.PrebaciZipFajlove();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"prebacivanje foldera: {ex.Message}");
            }

        }
    }
}
