
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
        public SpecifikacijaController(GlobosSftpService sftpService, VizimIntegracijaDb_Context db)
        {
            _sftpService = sftpService; 
            _db = db;
        }

        [HttpGet("upisiFajlove")]
        public IActionResult UpisiFajlove()
        {
            try
            {
                SpecifikacijaManager manager = new(_sftpService, _db);
                manager.UpisiFajlove();
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
                SpecifikacijaManager manager = new(_sftpService, _db);
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
