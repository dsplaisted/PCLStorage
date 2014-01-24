using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PCLTesting;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;


namespace PCLStorage.Test
{
	[TestClass]
    public class FileTestsForDesktop
    {
        IFileSystem TestFileSystem { get { return FileSystem.Current; } }

        [TestMethod]
        public void OpenFileForRead_NoHangForAsync()
        {
            // Simulate the UI thread of an app.
            UIDispatcherMock.MainThreadEntrypoint(async delegate
            {
                //  Arrange
                IFolder folder = TestFileSystem.LocalStorage;
                string fileName = "fileToOpenForRead.txt";
                IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

                //  Act
                using (Stream stream = await file.OpenAsync(FileAccess.Read))
                {

                    //  Assert
                    Assert.IsFalse(stream.CanWrite);
                    Assert.IsTrue(stream.CanRead);
                    Assert.IsTrue(stream.CanSeek);
                }

                //  Cleanup
                await file.DeleteAsync();
            });
        }

        [TestMethod]
        public void OpenFileForRead_NoHangForSync()
        {
            // Simulate the UI thread of an app.
            UIDispatcherMock.MainThreadEntrypoint(delegate
            {
                //  Arrange
                IFolder folder = TestFileSystem.LocalStorage;
                string fileName = "fileToOpenForRead.txt";
                IFile file = folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists).Result;

                //  Act
                using (Stream stream = file.OpenAsync(FileAccess.Read).Result)
                {

                    //  Assert
                    Assert.IsFalse(stream.CanWrite);
                    Assert.IsTrue(stream.CanRead);
                    Assert.IsTrue(stream.CanSeek);
                }

                //  Cleanup
                file.DeleteAsync().Wait();
            });
        }
    }
}
