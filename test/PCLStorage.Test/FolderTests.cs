using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
#if PORTABLE
using PCLStorage.Exceptions;
#endif
using PCLStorage.TestFramework;


namespace PCLStorage.Test
{
    [TestClass]
    public class FolderTests
    {
        [TestMethod]
        public async Task GetFile()
        {
            //  Arrange
            IFolder folder = Storage.AppLocalStorage;
            string fileName = "fileToGet.txt";
            IFile createdFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

            //  Act
            IFile gottenFile = await folder.GetFileAsync(fileName);

            //  Assert
            Assert.AreEqual(fileName, gottenFile.Name);

            //  Cleanup
            await createdFile.DeleteAsync();
        }

        [TestMethod]
        public async Task GetFileThatDoesNotExist()
        {
            //  Arrange
            IFolder folder = Storage.AppLocalStorage;
            string fileName = "fileThatDoesNotExist.txt";

            //  Act & Assert
            await ExceptionAssert.ThrowsAsync<FileNotFoundException>(async () => await folder.GetFileAsync(fileName));
        }

        [TestMethod]
        public async Task GetFilesEmpty()
        {
            //  Arrange
            IFolder folder = await Storage.AppLocalStorage.CreateFolderAsync("GetFilesEmpty_Folder", CreationCollisionOption.FailIfExists);

            //  Act
            IList<IFile> files = await folder.GetFilesAsync();

            //  Assert
            Assert.AreEqual(0, files.Count, "File count");

            //  Cleanup
            await folder.DeleteAsync();
        }

        [TestMethod]
        public async Task GetFilesSingle()
        {
            //  Arrange
            IFolder folder = await Storage.AppLocalStorage.CreateFolderAsync("GetFilesSingle_Folder", CreationCollisionOption.FailIfExists);
            await folder.CreateFileAsync("file.txt", CreationCollisionOption.FailIfExists);

            //  Act
            IList<IFile> files = await folder.GetFilesAsync();

            //  Assert
            Assert.AreEqual(1, files.Count, "File count");
            Assert.AreEqual("file.txt", files[0].Name);

            //  Cleanup
            await folder.DeleteAsync();
        }

        [TestMethod]
        public async Task GetFilesMultiple()
        {
            //  Arrange
            IFolder folder = await Storage.AppLocalStorage.CreateFolderAsync("GetFilesMultiple_Folder", CreationCollisionOption.FailIfExists);
            var fileNames = new[] { "hello.txt", "file.zzz", "anotherone", "42" };
            foreach (var fn in fileNames)
            {
                await folder.CreateFileAsync(fn, CreationCollisionOption.FailIfExists);
            }

            //  Act
            IList<IFile> files = await folder.GetFilesAsync();

            //  Assert
            Assert.AreEqual(fileNames.Length, files.Count, "File count");
            foreach (var fn in fileNames)
            {
                Assert.IsTrue(files.Count(f => f.Name == fn) == 1, "File " + fn + " in results");
            }

            //  Cleanup
            await folder.DeleteAsync();
        }

        [TestMethod]
        public async Task DeleteAppLocalStorageThrows()
        {
            //  Arrange
            IFolder folder = Storage.AppLocalStorage;

            //  Act & Assert
            await ExceptionAssert.ThrowsAsync<IOException>(async () => await folder.DeleteAsync());
        }
    }
}
