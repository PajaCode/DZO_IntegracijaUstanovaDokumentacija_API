
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
using HR_API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Messages;
using static System.Net.WebRequestMethods;
using DZO_IntegracijaUstanovaDokumentacija_API.Helpers;
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
        public IActionResult upisiFajlove()
        {
            try
            {
                SpecifikacijaManager manager = new(_sftpService, _db);
                manager.upisiFajlove();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }


        [HttpGet("parsirajIinsertuj")]
        public IActionResult parsirajIinsertuj()
        {
            try
            {
                SpecifikacijaManager manager = new(_sftpService, _db);
                manager.parsirajIinsertujAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }

        }


        

        //[HttpGet("folder")]
        //public IActionResult CreateFolderAndMoveFiles()
        //{
        //    try
        //    {
        //        IQueryable<Folderi> upit = db.DZOI_Vizim_Racun
        //                     .Join(db.DZOI_Vizim_Specifikacija,
        //                         rac => rac.IdSpecifikacije,
        //                         spec => spec.Id,
        //                         (rac, spec) => new { rac, spec })
        //                     .Where(rs => rs.spec.StatusId == 1)
        //                     .Select(rs => new Folderi
        //                     {
        //                        Uput= rs.rac.UputBroj,
        //                        SpecId= rs.spec.Id
        //                     });

        //        using SftpClient sftp = new(host, username, password);

        //        sftp.Connect();
        //        foreach(Folderi folder in upit)
        //        {
        //            string newFolderPath = remotePath+folder.Uput;
        //            if (!sftp.Exists(newFolderPath))
        //            {
        //                sftp.CreateDirectory(newFolderPath);
        //            }
        //        }

        //        //var files = sftp.ListDirectory(remotePath);

        //        // Copy each file to the new folder
        //        //foreach (var file in files)
        //        //{
        //        //    if (!file.IsDirectory && !file.IsSymbolicLink)
        //        //    {
        //        //        string sourceFilePath = remotePath + file.Name;
        //        //        string destinationFilePath = newFolderPath + "/" + file.Name;

        //        //        using (Stream fileStream = sftp.OpenRead(sourceFilePath))
        //        //        using (Stream newFileStream = sftp.Create(destinationFilePath))
        //        //        {
        //        //            fileStream.CopyTo(newFileStream);
        //        //        }
        //        //    }
        //        //}

        //        sftp.Disconnect();

        //        return Ok(new { message = "Files copied successfully." });
        //    }
        //    catch(Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while creating the folder.", error = ex.Message });
        //    }

        //}


    }
}
