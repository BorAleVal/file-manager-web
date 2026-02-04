namespace FileManagerWebAPI.Model
{
    public class FileData(string name, string size, string dateModified, string type)
    {
        public string Name { get; private set; } = name;
        public string Size { get; private set; } = size;
        public string DateModified { get; private set; } = dateModified;
        public string Type { get; private set; } = type;
    }
}
