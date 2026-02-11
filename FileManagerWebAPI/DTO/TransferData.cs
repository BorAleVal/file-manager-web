namespace FileManagerWebAPI.DTO
{
    public class TransferData
    {
        public string[] NameCollection { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; } = "";
        public bool IsCopy { get; set; } = false;
    }
}
