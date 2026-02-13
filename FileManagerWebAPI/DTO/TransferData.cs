namespace FileManagerWebAPI.DTO
{
    /// <summary>
    /// Класс для получения данных из запроса копирования/перемещения/удаления
    /// </summary>
    public class TransferData
    {
        /// <summary>
        /// Коллекция имён объектов для перемещения/копирования/удаления
        /// </summary>
        public string[] NameCollection { get; set; } = [];
        /// <summary>
        /// Директория из которой производится перемещение/копирование/удаления
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;
        /// <summary>
        /// Целевая директория куда будет произведено перемещение/копирование
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;
        /// <summary>
        /// Перемещение или копирование
        /// </summary>
        public bool IsCopy { get; set; } = false;
    }
}
