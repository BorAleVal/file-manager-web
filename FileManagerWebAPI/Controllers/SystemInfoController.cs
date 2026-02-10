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

        [HttpPost("copyElements")]
        public IActionResult CopyElements([FromBody]PathCollection pathCollection, string destinationDirPath)
        {
            var notExistedFile = pathCollection.Collection.FirstOrDefault(x => !Path.Exists(x));
            if (notExistedFile != null)
                //todo: перечислить файлы в сообщении
                return BadRequest($"файл {notExistedFile} не существует");

            var success = false;
            foreach (var sourcePath in pathCollection.Collection)
            {
                var isDirectory = FileTools.IsDirectory(sourcePath);
                if (isDirectory)
                {
                    success = CopyDirectory(sourcePath, destinationDirPath);
                }
                else
                {
                    success = FileTools.CopyFile(sourcePath, destinationDirPath);
                }
                if (!success)
                {
                    return BadRequest($"Не удалось скопировать {(isDirectory ? "файл" : "директорию")} {sourcePath}");
                }
            }

            return Ok();
        }

        public bool CopyDirectory(string sourcePath, string destinationDirPath)
        {

            try
            {
                CopyDirectory(sourcePath, destinationDirPath, recursive: true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpGet("move")]
        public IActionResult Move(string sourcePath, string destinationDirPath)
        {
            if (!Path.Exists(sourcePath))
                return BadRequest($"файл {sourcePath} не существует");

            try
            {
                Directory.Move(sourcePath, destinationDirPath);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("delete")]
        public IActionResult Delete(string path)
        {
            if (!Path.Exists(path))
                return BadRequest($"файл {path} не существует");

            try
            {
                DeleteFile(path);
                return Ok();

            }
            catch (Exception)
            {

                throw;
            }
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


        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private static void DeleteFile(string path)
        {
            new FileInfo(path).Delete();
        }

        private static void DeleteDirectory(string path)
        {
            new DirectoryInfo(path).Delete();
        }


    }
}
