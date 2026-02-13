using FileManagerWebAPI.Tools;

namespace FileManagerWebAPI.Tests
{
    [TestClass]
    public class PathToolsTest
    {
        [TestMethod]
        public void FileSizeTest()
        {
            var fs = new FileSize(1234567);
            Assert.AreEqual("1.2 Мб", fs.FormattedSize);
        }
    }
    //todo: не сделал тесты для методов работающих с файлами, в будущем надо разобраться как их сделать, по идее нужно сделать обертку над штатными средствами и сделать мок.
}
