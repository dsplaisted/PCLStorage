using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif DESKTOP
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
#if PORTABLE
using PCLStorage.Exceptions;
#endif

using PCLStorage.TestFramework;


namespace PCLStorage.Test
{
    [TestClass]
    public class FileSystemTests
    {
        IFileSystem TestFileSystem { get { return FileSystem.Current; } }

        [TestMethod]
        public async Task GetFileFromPath()
        {
            //  Arrange
            IFolder folder = await TestFileSystem.LocalStorage.CreateFolderAsync("GetFileFromPath_Folder", CreationCollisionOption.FailIfExists);
            string fileName = "file.txt";
            string fileContents = "Do you like green eggs and ham?";
            IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
            await file.WriteAllTextAsync(fileContents);

            string expectedPath = PortablePath.Combine(folder.Path, fileName);

            //  Act
            IFile gottenFile = await TestFileSystem.GetFileFromPathAsync(expectedPath);
            string gottenContents = await gottenFile.ReadAllTextAsync();

            //  Assert
            Assert.AreEqual(expectedPath, file.Path, "file.Path");
            Assert.AreEqual(expectedPath, gottenFile.Path, "gottenFile.Path");
            Assert.AreEqual(fileContents, gottenContents);

            //  Cleanup
            await folder.DeleteAsync();            
        }

        [TestMethod]
        public async Task GetFileFromPath_WhenFileDoesNotExist()
        {
            //  Arrange
            IFolder folder = await TestFileSystem.LocalStorage.CreateFolderAsync("GetFileFromPath_WhenFileDoesNotExist_Folder", CreationCollisionOption.FailIfExists);
            string pathForFile = PortablePath.Combine(folder.Path, "filethatdoesnotexist.txt");

            //  Act
            IFile file = await TestFileSystem.GetFileFromPathAsync(pathForFile);

            //  Assert
            Assert.AreEqual(null, file);

            //  Cleanup
            await folder.DeleteAsync();
        }

        //[TestMethod]
        //public async Task GetfileFromPath_WhenFolderDoesNotExist()
        //{

        //}
    }
}
