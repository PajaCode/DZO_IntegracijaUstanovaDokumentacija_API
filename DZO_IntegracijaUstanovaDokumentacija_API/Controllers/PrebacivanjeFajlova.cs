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
        public PrebacivanjeFajlova(GlobosSftpService sftpService, VizimIntegracijaDb_Context db)
        {
            _sftpService = sftpService;
            _db = db;
        }


        [HttpGet("kreirajFoldere")]
        public IActionResult KreirajFoldere()
        {
            try
            {
                PrebacivanjeFajlovaManager manager = new(_sftpService, _db);
                manager.kreirajFoldere();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }


        [HttpGet("prebaciFajlove")]
        public IActionResult PrebaciFajlove()
        {
            try
            {
                PrebacivanjeFajlovaManager manager = new(_sftpService, _db);
                manager.prebaciFajloveAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }



    }
}
