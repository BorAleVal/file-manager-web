namespace FileManagerWebAPI.DTO
{
    /// <summary>
    /// Класс для передачи данных в ответе
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="size">Размер</param>
    /// <param name="dateModified">Дата изменения</param>
    /// <param name="type">Тип файла (папка/расширение)</param>
    public class PathData(string name, string size, string dateModified, string type)
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; private set; } = name;

        /// <summary>
        /// Размер
        /// </summary>
        public string Size { get; private set; } = size;

        /// <summary>
        /// Дата изменения
        /// </summary>
        public string DateModified { get; private set; } = dateModified;

        /// <summary>
        /// Тип файла (папка/расширение)
        /// </summary>
        public string Type { get; private set; } = type;
    }
}
