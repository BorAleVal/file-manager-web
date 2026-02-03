using FileManagerWebAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileManagerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemInfoController : ControllerBase
    {
        [HttpGet("pathcontent")]
        public IActionResult PathContent(string path)
        {
            if (!Directory.Exists(path))
                return BadRequest("директория не существует");
            IEnumerable<FileData> content = GetPathContent(path);
            return Ok(content);
        }

        [HttpGet("ownerPath")]
        public IActionResult OwnerPath(string path)
        {
            if (!Directory.Exists(path))
                return BadRequest("директория не существует");
            var ownerPath = Path.GetDirectoryName(path);

            if (ownerPath == null)
                return BadRequest("директория не существует");

            return Ok(ownerPath);
        }

        private static IEnumerable<FileData> GetPathContent(string path)
        {
            var files = Directory.GetFiles(path).Select(x => new FileData(x, "1", "2"));
            var dirs = Directory.GetDirectories(path).Select(x => new FileData(x, "3", "5"));
            return dirs;
        }
    }
}
