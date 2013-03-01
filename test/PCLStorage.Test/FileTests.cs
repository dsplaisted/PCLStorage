using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCLStorage.TestFramework;

namespace PCLStorage.Test
{
	[TestClass]
    public class FileTests
    {
		[TestMethod]
		public static async Task GetFileThrowsWhenFileDoesNotExist()
		{
			string fileName = Guid.NewGuid().ToString();
			IFolder folder = Storage.AppLocalStorage;
			Exception ex = await Assert.ThrowsAsync<IOException>(async () => await folder.GetFileAsync(fileName));
			Assert.AreEqual("System.IO.FileNotFoundException", ex.GetType().FullName);
		}
    }
}
