using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DZO_IntegracijaUstanovaDokumentacija_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrebacivanjeFolderaCorisu : ControllerBase
    {
        [HttpGet("prebaciFoldere")]
        public IActionResult PrebaciFoldere()
        {
            try
            {
                PrebacivanjeFolderaCorisuManager manager = new();
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
