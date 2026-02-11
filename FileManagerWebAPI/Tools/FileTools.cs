using Microsoft.AspNetCore.Mvc;

namespace FileManagerWebAPI.Tools
{
    public class FileTools
    {
        public static bool IsDirectory(string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);

        public static bool CopyFile(string sourcePath, string destinationDirPath)
        {
            try
            {
                var fileName = Path.GetFileName(sourcePath);
                var copiedFullFileName = GetCopiedName(destinationDirPath, fileName);

                File.Copy(sourcePath, copiedFullFileName, false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetCopiedName(string destinationDirPath, string fileName)
        {
            var result = Path.Combine(destinationDirPath, fileName);
            if (Path.Exists(result))
            {
                result = GetCopiedName(destinationDirPath, $"copy-{fileName}");
            }
            return result;
        }

        public static bool CopyDirectory(string sourcePath, string destinationDirPath)
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

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            if (ContainsDirectory(sourceDir, destinationDir))
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

        public static bool MoveFile(string sourcePath, string destinationDirPath)
        {
            try
            {
                var fileName = Path.GetFileName(sourcePath);
                var copiedFullFileName = GetCopiedName(destinationDirPath, fileName);

                File.Move(sourcePath, copiedFullFileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool MoveDirectory(string sourceDir, string destinationDirPath)
        {
            try
            {
                if (ContainsDirectory(sourceDir, destinationDirPath))
                    throw new DirectoryNotFoundException($"Невозможно скопировать папку саму в себя: {sourceDir}");

                var dirName = Path.GetFileName(sourceDir);

                Directory.Move(sourceDir, Path.Combine(destinationDirPath, dirName));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ContainsDirectory(string path, string checkParentDirectory)
        {
            var dir = new DirectoryInfo(path);
            while (dir != null)
            {
                if (dir.Name.Equals(checkParentDirectory, StringComparison.CurrentCultureIgnoreCase))
                    return true;
                dir = dir.Parent;
            }
            return false;
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                new FileInfo(path).Delete();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteDirectory(string path)
        {
            try
            {
                new DirectoryInfo(path).Delete();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

    }
}
