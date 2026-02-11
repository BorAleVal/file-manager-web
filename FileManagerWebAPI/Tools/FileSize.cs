using System.Drawing;
using System.Globalization;

namespace FileManagerWebAPI.Tools
{
    /// <summary>
    /// Размер файла
    /// </summary>
    public class FileSize
    {
        /// <summary>
        /// Единицы измерения размера
        /// </summary>
        private readonly static string[] units = { "б", "Кб", "Мб", "Гб", "Тб", "Пб" };

        public FileSize(long size)
        {
            value = size;
            double calcSize = size;
            int i = 0;
            while (calcSize >= 1024 && i < units.Length - 1)
            {
                i++;
                calcSize = calcSize / 1024;
            }
            calculatedSize = calcSize;
            unit = units[i];
        }

        /// <summary>
        /// Значение в первоначальном виде
        /// </summary>
        private long value { get; }
        private double calculatedSize;
        private string unit;

        /// <summary>
        /// Отформатированное знаяение
        /// </summary>
        public string FormattedSize => $"{calculatedSize.ToString("0.#", CultureInfo.InvariantCulture)} {unit}";
    }
}
