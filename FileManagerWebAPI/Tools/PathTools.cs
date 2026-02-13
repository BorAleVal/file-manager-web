using FileManagerWebAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace FileManagerWebAPI.Tools
{
    public class PathTools
    {
        /// <summary>
        /// Определяет является заданный путь директорией или файлом
        /// </summary>
        /// <param name="path">путь</param>
        /// <returns></returns>
        public static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);

        /// <summary>
        /// Запуск файла
        /// </summary>
        /// <param name="filePath"></param>
        public static void RunFile(string filePath)
        {
            var peocess = new Process();
            peocess.StartInfo = new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            };
            peocess.Start();
        }

        /// <summary>
        /// Копирование файла
        /// </summary>
        /// <param name="sourcePath">путь копируемого файла</param>
        /// <param name="destinationDirPath">путь получаемого файла</param>
        /// <returns></returns>
        public static void CopyFile(string sourcePath, string destinationDirPath)
        {
            var fileName = Path.GetFileName(sourcePath);
            var copiedFullFileName = GetCopiedNameRecursive(destinationDirPath, fileName);

            File.Copy(sourcePath, copiedFullFileName, false);
        }

        /// <summary>
        /// Копирование директории (полное)
        /// </summary>
        /// <param name="sourcePath">путь копируемой директории</param>
        /// <param name="destinationDirPath">путь получаемой директории</param>
        /// <returns></returns>
        public static void CopyDirectory(string sourcePath, string destinationDirPath)
        {
            CopyDirectory(sourcePath, destinationDirPath, recursive: true);
        }


        /// <summary>
        /// Копирование директории (полное)
        /// </summary>
        /// <param name="sourcePath">путь копируемой директории</param>
        /// <param name="destinationDirPath">путь получаемой директории</param>
        /// <param name="recursive">рекурсивно</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            if (ContainsDirectory(destinationDir, sourceDir))
                throw new DirectoryNotFoundException($"Невозможно скопировать папку саму в себя: {sourceDir}");

            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Копируемая папка не существует: {dir.FullName}");

            var copiedDir = Path.Combine(destinationDir, dir.Name);

            if (!Directory.Exists(copiedDir))
            {
                Directory.CreateDirectory(copiedDir);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(copiedDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            if (recursive)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();

                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(copiedDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        /// <summary>
        /// Перемещение файла
        /// </summary>
        /// <param name="sourcePath">путь копируемой директории</param>
        /// <param name="destinationDirPath">путь получаемой директории</param>
        /// <returns></returns>
        public static void MoveFile(string sourcePath, string destinationDirPath)
        {
            var fileName = Path.GetFileName(sourcePath);
            var copiedFullFileName = GetCopiedNameRecursive(destinationDirPath, fileName);

            File.Move(sourcePath, copiedFullFileName);
        }

        /// <summary>
        /// Перемещение директории
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDirPath"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void MoveDirectory(string sourceDir, string destinationDirPath)
        {
            if (ContainsDirectory(destinationDirPath, sourceDir))
                throw new DirectoryNotFoundException($"Невозможно скопировать папку саму в себя: {sourceDir}");

            var dirName = Path.GetFileName(sourceDir);

            Directory.Move(sourceDir, Path.Combine(destinationDirPath, dirName));
        }

        /// <summary>
        /// Содержит ли директория другую директорию
        /// </summary>
        /// <param name="containerDir">директория верхнего уровня</param>
        /// <param name="path">директория, которая проверяется есть ли она в директории верхнего уровня</param>
        /// <returns></returns>
        public static bool ContainsDirectory(string containerDir, string path)
        {
            var dir = new DirectoryInfo(path);
            while (dir != null)
            {
                if (dir.FullName.Equals(containerDir, StringComparison.CurrentCultureIgnoreCase))
                    return true;

                dir = dir.Parent;
            }
            return false;
        }

        /// <summary>
        /// Удаляет файл
        /// </summary>
        /// <param name="path">путь</param>
        /// <returns></returns>
        public static bool DeleteFile(string path)
        {
            new FileInfo(path).Delete();
            return true;
        }

        /// <summary>
        /// Удаляет директорию
        /// </summary>
        /// <param name="path">путь</param>
        /// <returns></returns>
        public static bool DeleteDirectory(string path)
        {
            new DirectoryInfo(path).Delete(true);
            return true;
        }

        /// <summary>
        /// Получение содержимого директории
        /// </summary>
        /// <param name="directory">директория</param>
        /// <returns></returns>
        public static IEnumerable<PathData> GetContent(DirectoryInfo directory)
        {
            var files = directory.GetFiles().Select(x =>
                new PathData(
                    x.Name,
                    new FileSize(x.Length).FormattedSize,
                    FormateDate(x.LastWriteTime),
                    x.Extension));

            var dirs = directory.GetDirectories().Select(x =>
                new PathData(
                    x.Name,
                    "dir",
                    FormateDate(x.LastWriteTime),
                    "dir"));

            return dirs.Concat(files);
        }

        private static string FormateDate(DateTime date)
        {
            return date.ToString("dd.MM.yyyy hh:mm:ss");
        }

        //Метод получения имени с учетом копирования
        private static string GetCopiedNameRecursive(string destinationDirPath, string fileName)
        {
            var result = Path.Combine(destinationDirPath, fileName);
            if (Path.Exists(result))
            {
                result = GetCopiedNameRecursive(destinationDirPath, $"copy-{fileName}");
            }
            return result;
        }
    }
}
