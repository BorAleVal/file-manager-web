using FileManagerWebAPI.DTO;
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

namespace FileManagerWebAPI.Controllers
{
    /// <summary>
    /// Контроллер работающий с файлами и директориями системы.
    /// </summary>
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

            try
            {
                var content = PathTools.GetContent(directory);
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

            if (string.IsNullOrWhiteSpace(ownerPath))
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
                PathTools.RunFile(filePath);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Ну удалось запустить файл {filePath}: {ex.Message}");
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
        public IActionResult TransferElements([FromBody] TransferData transferData)
        {
            if (!Path.Exists(transferData.DestinationPath))
                return BadRequest($"файл {transferData.DestinationPath} не существует");

            var notExistedFile = transferData.NameCollection.FirstOrDefault(x => !Path.Exists(Path.Combine(transferData.SourcePath, x)));
            if (notExistedFile != null)
                return BadRequest($"файл {notExistedFile} не существует");

            try
            {
                foreach (var name in transferData.NameCollection)
                {
                    var sourcePath = Path.Combine(transferData.SourcePath, name);
                    var isDirectory = PathTools.IsDirectory(sourcePath);
                    if (transferData.IsCopy)
                    {
                        if (isDirectory)
                            PathTools.CopyDirectory(sourcePath, transferData.DestinationPath);
                        else
                            PathTools.CopyFile(sourcePath, transferData.DestinationPath);
                    }
                    else
                    {
                        if (isDirectory)
                            PathTools.MoveDirectory(sourcePath, transferData.DestinationPath);
                        else
                            PathTools.MoveFile(sourcePath, transferData.DestinationPath);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Не удалось {(transferData.IsCopy ? "скопировать" : "переместить")}:" +
                    $"\n {ex.Message}");
            }
        }

        [HttpPost("deleteElements")]
        public IActionResult DeleteElements([FromBody] TransferData transferData)
        {
            var notExistedFile = transferData.NameCollection.FirstOrDefault(x => !Path.Exists(Path.Combine(transferData.SourcePath, x)));
            if (notExistedFile != null)
                return BadRequest($"файл {notExistedFile} не существует");

            try
            {
                foreach (var name in transferData.NameCollection)
                {
                    var sourcePath = Path.Combine(transferData.SourcePath, name);
                    var isDirectory = PathTools.IsDirectory(sourcePath);
                    if (isDirectory)
                        PathTools.DeleteDirectory(sourcePath);
                    else
                        PathTools.DeleteFile(sourcePath);
                }

                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest($"Не удалось удалить:\n{ex.Message}");
           }
        }
    }
}
