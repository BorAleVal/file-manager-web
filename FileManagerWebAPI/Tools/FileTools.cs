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

    }
}
