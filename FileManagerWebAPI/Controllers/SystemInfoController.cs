using FileManagerWebAPI.DTO;
using FileManagerWebAPI.Model;
using FileManagerWebAPI.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileManagerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemInfoController : ControllerBase
    {
        [HttpGet("directoryContent")]
        public IActionResult DirectoryContent(string path)
        {
            var directory = new DirectoryInfo(path);

            if (!directory.Exists)
                return BadRequest(new { message = "директория не существует" });

            //todo: добавить проверку на доступ на чтение
            try
            {
                IEnumerable<FileData> content = GetPathContent(directory);
                return Ok(content);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
                return BadRequest($"файл {filePath} не существует");

            try
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true
                };
                p.Start();

                return Ok();

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("hardDrives")]
        public IActionResult HardDrives()
        {
            var drives = DriveInfo.GetDrives();
            var result = drives.Where(x => x.IsReady).Select(x => x.Name);
            return Ok(result);
        }

        [HttpPost("transferElements")]
        public IActionResult transferElements([FromBody] TransferData transferData)
        {
            if (!Path.Exists(transferData.DestinationPath))
                return BadRequest($"файл {transferData.DestinationPath} не существует");

            var notExistedFile = transferData.NameCollection.FirstOrDefault(x => !Path.Exists(Path.Combine(transferData.SourcePath, x)));
            if (notExistedFile != null)
                return BadRequest($"файл {notExistedFile} не существует");

            var success = false;
            foreach (var name in transferData.NameCollection)
            {
                var sourcePath = Path.Combine(transferData.SourcePath, name);
                var isDirectory = FileTools.IsDirectory(sourcePath);
                if (transferData.IsCopy)
                {
                    success = isDirectory
                        ? FileTools.CopyDirectory(sourcePath, transferData.DestinationPath)
                        : FileTools.CopyFile(sourcePath, transferData.DestinationPath);
                }
                else
                {
                    //success = FileTools.MoveFile(sourcePath, transferData.DestinationPath); 
                    success =isDirectory
                        ? FileTools.MoveDirectory(sourcePath, transferData.DestinationPath)
                        : FileTools.MoveFile(sourcePath, transferData.DestinationPath);
                }
                if (!success)
                {
                    return BadRequest($"Не удалось скопировать {(isDirectory ? "файл" : "директорию")} {sourcePath}");
                }
            }

            return Ok();
        }

        [HttpPost("deleteElements")]
        public IActionResult DeleteElements([FromBody] TransferData transferData)
        {
            var notExistedFile = transferData.NameCollection.FirstOrDefault(x => !Path.Exists(Path.Combine(transferData.SourcePath, x)));
            if (notExistedFile != null)
                return BadRequest($"файл {notExistedFile} не существует");

            var success = false;
            foreach (var name in transferData.NameCollection)
            {
                var sourcePath = Path.Combine(transferData.SourcePath, name);
                var isDirectory = FileTools.IsDirectory(sourcePath);
                    success = isDirectory
                        ? FileTools.DeleteDirectory(sourcePath)
                        : FileTools.DeleteFile(sourcePath);
                if (!success)
                {
                    return BadRequest($"Не удалось скопировать {(isDirectory ? "файл" : "директорию")} {sourcePath}");
                }
            }

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
