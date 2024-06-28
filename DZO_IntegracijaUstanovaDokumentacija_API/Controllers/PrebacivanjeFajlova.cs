using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrebacivanjeFajlova : ControllerBase
    {
        private readonly GlobosSftpService _sftpService;
        private readonly VizimIntegracijaDb_Context _db;
        private readonly Logovi _logger;
        public PrebacivanjeFajlova(GlobosSftpService sftpService, VizimIntegracijaDb_Context db, Logovi logger)
        {
            _sftpService = sftpService;
            _db = db;
            _logger = logger;   
        }


        [HttpGet("kreirajFoldere")]
        public IActionResult KreirajFoldere()
        {
            try
            {
                PrebacivanjeFajlovaManager manager = new(_sftpService, _db, _logger);
                manager.KreirajFoldere();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"kreiranje foldera: {ex.Message}");
            }

        }


        [HttpGet("prebaciFajlove")]
        public IActionResult PrebaciFajlove()
        {
            try
            {
                PrebacivanjeFajlovaManager manager = new(_sftpService, _db, _logger);
                manager.PrebaciFajloveAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"prebacivanje fajlova: {ex.Message}");
            }

        }



    }
}
