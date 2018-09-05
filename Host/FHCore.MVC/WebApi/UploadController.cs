using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
namespace FHCore.MVC.WebApi
{
    [Route("api/[controller]")]
    public class UploadController:ControllerBase
    {
        public async Task<IActionResult> Post(List<IFormFile> file)
        {
            
            foreach(IFormFile item in file)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(),"images",item.FileName);
                if(item.Length>0)
                {
                    using(var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await item.CopyToAsync(stream);
                    }
                }
            }
            return Ok(new { count = file.Count});
        }
    }
}