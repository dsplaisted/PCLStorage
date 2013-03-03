using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
using PCLStorage.TestFramework;


namespace PCLStorage.Test
{
	[TestClass]
    public class FileTests
    {
		[TestMethod]
		public async Task GetFileThrowsWhenFileDoesNotExist()
		{
			string fileName = Guid.NewGuid().ToString();
			IFolder folder = Storage.AppLocalStorage;
			Exception ex = await ExceptionAssert.ThrowsAsync<IOException>(async () => await folder.GetFileAsync(fileName));
			Assert.AreEqual("System.IO.FileNotFoundException", ex.GetType().FullName);
		}

        [TestMethod]
        public async Task CreateFile()
        {
            //  Arrange
            IFolder folder = Storage.AppLocalStorage;

            //  Act
            IFile file = await folder.CreateFileAsync("foo.txt", CreationCollisionOption.FailIfExists);

            //  Assert
            Assert.AreEqual("foo.txt", file.Name);

            //using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite))
            //{
            //    using (var sw = new StreamWriter(stream))
            //    {
            //        await sw.WriteLineAsync("Hello, World");
            //    }
            //}

            //  Cleanup
            await file.DeleteAsync();
        }
    }
}
