using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using PCLTesting;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace PCLStorage.Test
{
    [TestClass]
    public class FileSystemTests
    {
        IFileSystem TestFileSystem { get { return FileSystem.Current; } }

        [TestMethod]
        public void LocalStorageExists()
        {
            Assert.IsFalse(TestFileSystem.LocalStorage == null);
        }

        [TestMethod]
        public void LocalStorageAndRoamingStorageHaveDifferentPaths()
        {
            if (TestFileSystem.RoamingStorage == null)
            {
                //  Not all platforms (specifically Silverlight/Windows Phone) implement roaming storage
                return;
            }

            //  Act
            string localPath = TestFileSystem.LocalStorage.Path;
            string roamingPath = TestFileSystem.RoamingStorage.Path;

            //  Assert
            Assert.IsFalse(localPath == roamingPath, "Roaming path should not equal local path: " + localPath);
        }

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

        [TestMethod]
        public async Task GetFileFromPath_WhenFolderDoesNotExist()
        {
            //  Arrange
            IFolder rootFolder = TestFileSystem.LocalStorage;
            string pathForFile = PortablePath.Combine(rootFolder.Path, "NotAFolder", "file.txt");

            //  Act
            IFile file = await TestFileSystem.GetFileFromPathAsync(pathForFile);

            //  Assert
            Assert.AreEqual(null, file);
        }

        [TestMethod]
        public void GetFileFromPath_Null()
        {
            Task result = TestFileSystem.GetFileFromPathAsync(null);
            Assert.IsTrue(result.IsFaulted);
            Assert.IsTrue(result.Exception.InnerException is ArgumentNullException);
        }

        [TestMethod]
        public async Task GetFolderFromPath()
        {
            //  Arrange
            IFolder folder = await TestFileSystem.LocalStorage.CreateFolderAsync("GetFolderFromPath_Folder", CreationCollisionOption.FailIfExists);
            string fileName1 = "file1.txt";
            string fileName2 = "file2.txt";
            await folder.CreateFileAsync(fileName1, CreationCollisionOption.FailIfExists);
            IFile file2 = await folder.CreateFileAsync(fileName2, CreationCollisionOption.FailIfExists);
            await file2.WriteAllTextAsync("File 2 contents");

            string expectedPath = folder.Path;

            //  Act
            IFolder gottenFolder = await TestFileSystem.GetFolderFromPathAsync(expectedPath);
            var files = await gottenFolder.GetFilesAsync();
            var fileNames = files.Select(f => f.Name);

            //  Assert
            Assert.AreEqual(expectedPath, gottenFolder.Path, "gottenFolder.Path");
            Assert.AreEqual(2, files.Count, "file count");
            Assert.IsTrue(fileNames.Contains("file1.txt"));
            Assert.IsTrue(fileNames.Contains("file2.txt"));

            //  Cleanup
            await gottenFolder.DeleteAsync();
        }

        [TestMethod]
        public async Task GetFolderFromPath_WhenFolderDoesNotExist()
        {
            //  Arrange
            IFolder rootFolder = TestFileSystem.LocalStorage;
            string pathForFolder = PortablePath.Combine(rootFolder.Path, "FolderThatDoesNotExist");

            //  Act
            IFolder folder = await TestFileSystem.GetFolderFromPathAsync(pathForFolder);

            //  Assert
            Assert.AreEqual(null, folder);
        }

        [TestMethod]
        public void GetFolderFromPath_Null()
        {
            Task result = TestFileSystem.GetFolderFromPathAsync(null);
            Assert.IsTrue(result.IsFaulted);
            Assert.IsTrue(result.Exception.InnerException is ArgumentNullException);
        }
    }
}
