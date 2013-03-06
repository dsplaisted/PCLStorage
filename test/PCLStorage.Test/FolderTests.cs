using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Assert.AreEqual(PortablePath.Combine(folder.Path, fileName), gottenFile.Path);

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
        public async Task CreateFolder()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string folderName = "FolderToCreate";

            //  Act
            IFolder folder = await rootFolder.CreateFolderAsync(folderName, CreationCollisionOption.FailIfExists);

            //  Assert
            Assert.AreEqual(folderName, folder.Name);
            Assert.AreEqual(PortablePath.Combine(rootFolder.Path, folderName), folder.Path, "Folder path");

            //  Cleanup
            await folder.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateNestedFolder()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string subFolderName = "NestedSubFolder";
            IFolder subFolder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.FailIfExists);
            string leafFolderName = "NestedLeafFolder";

            //  Act
            IFolder testFolder = await subFolder.CreateFolderAsync(leafFolderName, CreationCollisionOption.FailIfExists);

            //  Assert
            Assert.AreEqual(leafFolderName, testFolder.Name);
            Assert.AreEqual(PortablePath.Combine(rootFolder.Path, subFolderName, leafFolderName), testFolder.Path, "Leaf folder path");

            //  Cleanup
            await subFolder.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFolderCollision_GenerateUniqueName()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string subFolderName = "Collision_Unique";
            IFolder existingFolder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.FailIfExists);

            //  Act
            IFolder folder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.GenerateUniqueName);

            //  Assert
            Assert.AreEqual(subFolderName + " (2)", folder.Name);

            //  Cleanup
            await existingFolder.DeleteAsync();
            await folder.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFolderCollision_ReplaceExisting()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string subFolderName = "Collision_Replace";
            IFolder existingFolder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.FailIfExists);
            await existingFolder.CreateFileAsync("FileInFolder.txt", CreationCollisionOption.FailIfExists);

            //  Act
            IFolder newFolder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.ReplaceExisting);

            //  Assert
            Assert.AreEqual(subFolderName, newFolder.Name);
            var files = await newFolder.GetFilesAsync();
            Assert.AreEqual(0, files.Count, "New folder file count");

            //  Cleanup
            await newFolder.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFolderCollision_FailIfExists()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string subFolderName = "Collision_Fail";
            IFolder existingFolder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.FailIfExists);

            //  Act & Assert
            await ExceptionAssert.ThrowsAsync<IOException>(async () =>
                {
                    await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.FailIfExists);
                });

            //  Cleanup
            await existingFolder.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFolderCollision_OpenIfExists()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string subFolderName = "Collision_Open";
            IFolder existingFolder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.FailIfExists);
            await existingFolder.CreateFileAsync("FileInFolder.txt", CreationCollisionOption.FailIfExists);

            //  Act
            IFolder newFolder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.OpenIfExists);

            //  Assert
            Assert.AreEqual(subFolderName, newFolder.Name);
            var files = await newFolder.GetFilesAsync();
            Assert.AreEqual(1, files.Count);
            Assert.AreEqual("FileInFolder.txt", files[0].Name);

            //  Cleanup
            await newFolder.DeleteAsync();
        }

        [TestMethod]
        public async Task GetFolder()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string folderName = "FolderToGet";
            IFolder createdFolder = await rootFolder.CreateFolderAsync(folderName, CreationCollisionOption.FailIfExists);

            //  Act
            IFolder gottenFolder = await rootFolder.GetFolderAsync(folderName);

            //  Assert
            Assert.AreEqual(folderName, gottenFolder.Name);
            Assert.AreEqual(PortablePath.Combine(rootFolder.Path, folderName), gottenFolder.Path);

            //  Cleanup
            await gottenFolder.DeleteAsync();
        }

        [TestMethod]
        public async Task GetFolderThatDoesNotExist()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string folderName = "FolderThatDoesNotExist";

            //  Act & Assert
            await ExceptionAssert.ThrowsAsync<PCLStorage.Exceptions.DirectoryNotFoundException>(
                async () => await rootFolder.GetFolderAsync(folderName));
        }

        [TestMethod]
        public async Task GetFoldersEmpty()
        {
            //  Arrange
            IFolder folder = await Storage.AppLocalStorage.CreateFolderAsync("GetFoldersEmpty_Folder", CreationCollisionOption.FailIfExists);

            //  Act
            IList<IFolder> folders = await folder.GetFoldersAsync();

            //  Assert
            Assert.AreEqual(0, folders.Count, "Folder count");

            //  Cleanup
            await folder.DeleteAsync();
        }

        [TestMethod]
        public async Task GetFoldersSingle()
        {
            //  Arrange
            IFolder folder = await Storage.AppLocalStorage.CreateFolderAsync("GetFoldersSingle_Folder", CreationCollisionOption.FailIfExists);
            string expectedFolderName = "Subfolder";
            await folder.CreateFolderAsync(expectedFolderName, CreationCollisionOption.FailIfExists);

            //  Act
            IList<IFolder> folders = await folder.GetFoldersAsync();

            //  Assert
            Assert.AreEqual(1, folders.Count, "Folder count");
            Assert.AreEqual(expectedFolderName, folders[0].Name);

            //  Cleanup
            await folder.DeleteAsync();
        }

        [TestMethod]
        public async Task GetFoldersMultiple()
        {
            //  Arrange
            IFolder folder = await Storage.AppLocalStorage.CreateFolderAsync("GetFoldersMultiple_Folder", CreationCollisionOption.FailIfExists);
            var folderNames = new[] { "One", "2", "Hello" };
            foreach (var fn in folderNames)
            {
                await folder.CreateFolderAsync(fn, CreationCollisionOption.FailIfExists);
            }

            //  Act
            IList<IFolder> folders = await folder.GetFoldersAsync();

            //  Assert
            Assert.AreEqual(folderNames.Length, folders.Count, "Folder count");
            foreach (var fn in folderNames)
            {
                Assert.IsTrue(folders.Count(f => f.Name == fn) == 1, "Folder " + fn + " in results");
            }

            //  Cleanup
            await folder.DeleteAsync();
        }

        [TestMethod]
        public async Task DeleteFolder()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            string subFolderName = "FolderToDelete";
            IFolder folder = await rootFolder.CreateFolderAsync(subFolderName, CreationCollisionOption.FailIfExists);

            //  Act
            await folder.DeleteAsync();

            //  Assert
            var folders = await rootFolder.GetFoldersAsync();
            Assert.IsFalse(folders.Any(f => f.Name == subFolderName));
        }

        [TestMethod]
        public async Task CreateFileInDeletedFolder()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            IFolder folder = await rootFolder.CreateFolderAsync("FolderToDeleteAndThenCreateFileIn", CreationCollisionOption.FailIfExists);
            await folder.DeleteAsync();

            //  Act & Assert
            await ExceptionAssert.ThrowsAsync<Exceptions.DirectoryNotFoundException>(async () =>
                { await folder.CreateFileAsync("Foo.txt", CreationCollisionOption.GenerateUniqueName); });
        }

        [TestMethod]
        public async Task DeleteFolderTwice()
        {
            //  Arrange
            IFolder rootFolder = Storage.AppLocalStorage;
            IFolder folder = await rootFolder.CreateFolderAsync("FolderToDeleteTwice", CreationCollisionOption.FailIfExists);
            await folder.DeleteAsync();

            //  Act & Asserth
            await ExceptionAssert.ThrowsAsync<Exceptions.DirectoryNotFoundException>(async () => await folder.DeleteAsync());
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
