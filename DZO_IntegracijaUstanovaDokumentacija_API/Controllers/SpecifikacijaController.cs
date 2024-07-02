
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using HR_API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Messages;
using static System.Net.WebRequestMethods;
using Microsoft.Extensions.Options;


namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecifikacijaController: ControllerBase
    {

        private readonly GlobosSftpService _sftpService;
        private readonly VizimIntegracijaDb_Context _db;
        private readonly Logovi _logger;
        public SpecifikacijaController(GlobosSftpService sftpService, VizimIntegracijaDb_Context db, Logovi logger)
        {
            _sftpService = sftpService; 
            _db = db;
            _logger = logger;
        }

        [HttpGet("upisiJsone")]
        public IActionResult UpisiJsone()
        {
            try
            {
                SpecifikacijaManager manager = new(_sftpService, _db, _logger);
                manager.UpisiJsone();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }


        [HttpGet("parsirajIinsertuj")]
        public IActionResult ParsirajIinsertuj()
        {
            try
            {
                SpecifikacijaManager manager = new(_sftpService, _db, _logger);
                manager.ParsirajIinsertujAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }


       


    }
}
