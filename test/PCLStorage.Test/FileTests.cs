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
    public class FileTests
    {
        public async Task PCLStorageSample()
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            IFolder folder = await rootFolder.CreateFolderAsync("MySubFolder",
                CreationCollisionOption.OpenIfExists);
            IFile file = await folder.CreateFileAsync("answer.txt",
                CreationCollisionOption.ReplaceExisting);
            await file.WriteAllTextAsync("42");
        }

        IFileSystem TestFileSystem { get { return FileSystem.Current; } }

		[TestMethod]
		public async Task GetFileThrowsWhenFileDoesNotExist()
		{
			string fileName = Guid.NewGuid().ToString();
            IFolder folder = TestFileSystem.LocalStorage;
			await ExceptionAssert.ThrowsAsync<FileNotFoundException>(async () => await folder.GetFileAsync(fileName));
		}

        [TestMethod]
        public async Task CreateFile()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName = "fileToCreate.txt";

            //  Act
            IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

            //  Assert
            Assert.AreEqual(fileName, file.Name);
            Assert.AreEqual(PortablePath.Combine(folder.Path, fileName), file.Path, "File Path");

            //  Cleanup
            await file.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFileSubFolder()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string subFolderName = "CreateFileSubFolder";
            IFolder subFolder = await folder.CreateFolderAsync(subFolderName, CreationCollisionOption.FailIfExists);
            string fileName = "fileToCreateInSubFolder.txt";

            //  Act
            IFile file = await subFolder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

            //  Assert
            Assert.AreEqual(fileName, file.Name);
            Assert.AreEqual(PortablePath.Combine(folder.Path, subFolderName, fileName), file.Path, "File Path");

            //  Cleanup
            await file.DeleteAsync();
            await subFolder.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFileNameCollision_GenerateUniqueName()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string baseFileName = "Collision_Unique";
            IFile file1 = await folder.CreateFileAsync(baseFileName + ".txt", CreationCollisionOption.FailIfExists);

            //  Act
            IFile file2 = await folder.CreateFileAsync(baseFileName + ".txt", CreationCollisionOption.GenerateUniqueName);

            //  Assert
            Assert.AreEqual(baseFileName + " (2).txt", file2.Name);

            //  Cleanup
            await file1.DeleteAsync();
            await file2.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFileNameCollision_ReplaceExisting()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName = "Collision_Replace.txt";
            IFile file1 = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
            await file1.WriteAllTextAsync("Hello, World");

            //  Act
            IFile file2 = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            //  Assert
            Assert.AreEqual(fileName, file2.Name);
            string file2Contents = await file2.ReadAllTextAsync();
            Assert.AreEqual(string.Empty, file2Contents);

            //  Cleanup
            await file2.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFileNameCollision_FailIfExists()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName = "Collision_Fail.txt";
            IFile file1 = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

            //  Act & Assert
            await ExceptionAssert.ThrowsAsync<IOException>(async () =>
                {
                    await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
                });

            //  Cleanup
            await file1.DeleteAsync();
        }

        [TestMethod]
        public async Task CreateFileNameCollision_OpenIfExists()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName = "Collision_OpenIfExists.txt";
            IFile file1 = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
            string contents = "Hello, World!";
            await file1.WriteAllTextAsync(contents);

            //  Act
            IFile file2 = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

            //  Assert
            Assert.AreEqual(fileName, file2.Name);
            string file2Contents = await file2.ReadAllTextAsync();
            Assert.AreEqual(contents, file2Contents);

            //  Cleanup
            await file2.DeleteAsync();
        }

		[TestMethod]
		public async Task WriteAndReadFile()
		{
			//	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
			IFile file = await folder.CreateFileAsync("readWriteFile.txt", CreationCollisionOption.FailIfExists);
			string contents = "And so we beat on, boats against the current, born back ceaselessly into the past.";

			//	Act
			await file.WriteAllTextAsync(contents);
			string readContents = await file.ReadAllTextAsync();

			//	Assert
			Assert.AreEqual(contents, readContents);

			//	Cleanup
			await file.DeleteAsync();
		}

		[TestMethod]
		public async Task DeleteFile()
		{
			//	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
			string fileName = "fileToDelete.txt";
			IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

			//	Act
			await file.DeleteAsync();

			//	Assert
			var files = await folder.GetFilesAsync();
			Assert.IsFalse(files.Any(f => f.Name == fileName));
		}

		[TestMethod]
		public async Task OpenDeletedFile()
		{
			//	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
			string fileName = "fileToDeleteAndOpen.txt";
			IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
			await file.DeleteAsync();

			//	Act & Assert
			await ExceptionAssert.ThrowsAsync<IOException>(async () => { await file.OpenAsync(FileAccess.ReadAndWrite); });
		}

		[TestMethod]
		public async Task DeleteFileTwice()
		{
			//	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
			string fileName = "fileToDeleteTwice.txt";
			IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
			await file.DeleteAsync();

			//	Act & Assert
			await ExceptionAssert.ThrowsAsync<IOException>(async () => { await file.DeleteAsync(); });
		}

        [TestMethod]
        public async Task OpenFileForRead()
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
        }

        [TestMethod]
        public async Task OpenFileForReadAndWrite()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName = "fileToOpenForReadAndWrite";
            IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);

            //  Act
            using (Stream stream = await file.OpenAsync(FileAccess.ReadAndWrite))
            {

                //  Assert
                Assert.IsTrue(stream.CanWrite);
                Assert.IsTrue(stream.CanRead);
                Assert.IsTrue(stream.CanSeek);                
            }

            //  Cleanup
            await file.DeleteAsync();
        }

        [TestMethod]
        public async Task NestedFolderWithSameName()
        {
            //  Arrange
            IFolder rootFolder = TestFileSystem.LocalStorage;
            string folderName = "NestedFolderName";
            IFolder level1 = await rootFolder.CreateFolderAsync(folderName, CreationCollisionOption.FailIfExists);
            
            //  Act
            IFolder level2 = await level1.CreateFolderAsync(folderName, CreationCollisionOption.FailIfExists);

            //  Assert
            Assert.AreEqual(PortablePath.Combine(rootFolder.Path, folderName, folderName), level2.Path);
            IList<IFolder> level1Folders = await level1.GetFoldersAsync();
            IList<IFolder> level2Folders = await level2.GetFoldersAsync();
            Assert.AreEqual(1, level1Folders.Count, "Level 1 folder count");
            Assert.AreEqual(0, level2Folders.Count, "Level 2 folder count");

            //  Cleanup
            await level1.DeleteAsync();
        }

        [TestMethod]
        public async Task SiblingFoldersContentsDiffer()
        {
            //  Arrange
            IFolder rootFolder = TestFileSystem.LocalStorage;
            IFolder siblingFolder1 = await rootFolder.CreateFolderAsync("SiblingFolder1", CreationCollisionOption.FailIfExists);
            IFolder siblingFolder2 = await rootFolder.CreateFolderAsync("SiblingFolder2", CreationCollisionOption.FailIfExists);
            string fileName = "file.txt";
            string contents1 = "This is the first file";
            string contents2 = "This is the second file";

            //  Act
            IFile file1 = await siblingFolder1.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
            await file1.WriteAllTextAsync(contents1);
            IFile file2 = await siblingFolder2.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
            await file2.WriteAllTextAsync(contents2);

            //  Assert
            file1 = await siblingFolder1.GetFileAsync(fileName);
            file2 = await siblingFolder2.GetFileAsync(fileName);
            string actualContents1 = await file1.ReadAllTextAsync();
            string actualContents2 = await file2.ReadAllTextAsync();
            Assert.AreEqual(contents1, actualContents1);
            Assert.AreEqual(contents2, actualContents2);

            //  Cleanup
            await siblingFolder1.DeleteAsync();
            await siblingFolder2.DeleteAsync();
        }

        [TestMethod]
        public async Task WriteAllTextOverwritesExistingContents()
        {
            //  Arrange
            IFolder rootFolder = TestFileSystem.LocalStorage;
            IFolder testFolder = await rootFolder.CreateFolderAsync("WriteAllTextFolder", CreationCollisionOption.FailIfExists);
            IFile testFile = await testFolder.CreateFileAsync("testfile.txt", CreationCollisionOption.FailIfExists);
            await testFile.WriteAllTextAsync("A man a plan a canal panama!");

            //  Act
            await testFile.WriteAllTextAsync("42");

            //  Assert
            string contents = await testFile.ReadAllTextAsync();
            Assert.AreEqual("42", contents);

            //  Cleanup
            await testFolder.DeleteAsync();
        }
    }
}
