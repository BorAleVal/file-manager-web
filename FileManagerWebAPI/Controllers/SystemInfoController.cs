using FileManagerWebAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FileManagerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemInfoController : ControllerBase
    {
        [HttpGet("pathcontent")]
        public IActionResult PathContent(string path)
        {
            var directory = new DirectoryInfo(path);
            
            if (!directory.Exists)
                return BadRequest("директория не существует");
            IEnumerable<FileData> content = GetPathContent(directory);
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

        [HttpGet("runFile")]
        public IActionResult RunFile(string filePath)
        {
            if (!Path.Exists(filePath))
                return BadRequest("файл не существует");
            
            //var iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName);
            Process.Start(filePath);

            return Ok();
        }

        private static IEnumerable<FileData> GetPathContent(DirectoryInfo directory)
        {
            var files = directory.GetFiles().Select(x => 
                new FileData(
                    x.Name,
                    GetFormattedSize(x.Length),
                    GetUserFriendlyDateString(x.LastWriteTime),
                    x.Extension));
            var dirs = directory.GetDirectories().Select(x =>
                new FileData(
                    x.Name,
                    "",
                    GetUserFriendlyDateString(x.LastWriteTime),
                    "folder"));
            return dirs.Concat(files);
        }

        private static string GetFormattedSize(long size)
        {
            if (size < 0)
                return "undefined";

            //todo: вынести в ридонли поле
            string[] units = { "б", "Кб", "Мб", "Гб", "Тб", "Пб" };
            double formattedSize = size;
            int i = 0;
            while (formattedSize >= 1024 && i < units.Length - 1)
            {
                i++;
                formattedSize = formattedSize / 1024;
            }
            var formattedSizeStr = formattedSize.ToString("0.#", CultureInfo.InvariantCulture);
            return $"{formattedSizeStr} {units[i]}";
        }

        public static string GetUserFriendlyDateString(DateTime date)
        {
            return date.ToString("dd.MM.yyyy hh:mm:ss");
        }
    }
}
