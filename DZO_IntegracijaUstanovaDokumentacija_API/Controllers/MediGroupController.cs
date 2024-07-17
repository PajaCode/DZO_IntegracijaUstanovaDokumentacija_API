using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using DZO_IntegracijaUstanovaDokumentacija_API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediGroupController : ControllerBase
    {
        private readonly MediGroupSftpService _sftpService;
        public MediGroupController(MediGroupSftpService sftpService)
        {
            _sftpService = sftpService;
        }
        [HttpGet("prebaciFoldere")]
        public IActionResult PrebaciFoldere()
        {
            try
            {
                MediGroupManager manager = new(_sftpService);
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
