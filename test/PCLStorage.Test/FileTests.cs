using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PCLTesting;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;


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
        public async Task AppendFile()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName = "fileToAppend.txt";
            
            //  Act
            IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
            await file.AppendAllTextAsync("Test");
            var text = await file.ReadAllTextAsync();

            //  Assert
            Assert.AreEqual("Test", text);

            // Act
            await file.AppendAllTextAsync("Test");
            text = await file.ReadAllTextAsync();

            //  Assert
            Assert.AreEqual("TestTest", text);


            //  Cleanup
            await file.DeleteAsync();
        }

        [TestMethod]
        public async Task AppendLines()
        {
            //  Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName = "fileToAppend.txt";
            string expected = "Test" + Environment.NewLine + "Test" + Environment.NewLine;

            //  Act
            IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await file.AppendAllLinesAsync("Test", "Test");
            var text = await file.ReadAllTextAsync();

            //  Assert
            Assert.AreEqual(expected, text);

            // Act
            var lines = new List<string>(){"Test", "Test"};
            await file.AppendAllLinesAsync(lines);
            expected += expected;
            text = await file.ReadAllTextAsync();

            //  Assert
            Assert.AreEqual(expected, text);


            //  Cleanup
            await file.DeleteAsync();
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

        [TestMethod]
        public async Task RenameFile()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "fileToRename.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);

            //	Act
            string renamedFile = "renamedFile.txt";
            await file.RenameAsync(renamedFile);

            //	Assert
            Assert.AreEqual(renamedFile, file.Name);
            Assert.AreEqual(PortablePath.Combine(TestFileSystem.LocalStorage.Path, file.Name), file.Path);
            var files = await folder.GetFilesAsync();
            Assert.IsFalse(files.Any(f => f.Name == originalFileName));
            Assert.IsTrue(files.Any(f => f.Name == renamedFile));

            // Cleanup
            await file.DeleteAsync();
        }

        [TestMethod]
        public async Task RenameFile_FailIfExists()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "fileToRename.txt";
            string renamedFile = "renamedFile.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);
            IFile existingFile = await folder.CreateFileAsync(renamedFile, CreationCollisionOption.FailIfExists);

            //	Act & Assert
            await ExceptionAssert.ThrowsAsync<IOException>(async () => await file.RenameAsync(renamedFile, NameCollisionOption.FailIfExists));
            Assert.AreEqual(originalFileName, file.Name);

            var files = await folder.GetFilesAsync();
            Assert.IsTrue(files.Any(f => f.Name == renamedFile));
            Assert.IsTrue(files.Any(f => f.Name == originalFileName));

            // Cleanup
            await file.DeleteAsync();
            await existingFile.DeleteAsync();
        }

        [TestMethod]
        public async Task RenameFile_GenerateUniqueName()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName1 = "file1.txt";
            string fileName2 = "file2.txt";
            string fileName1_renamed = "file2 (2).txt";
            IFile file1 = await folder.CreateFileAsync(fileName1, CreationCollisionOption.FailIfExists);
            IFile file2 = await folder.CreateFileAsync(fileName2, CreationCollisionOption.FailIfExists);

            //	Act
            await file1.RenameAsync(fileName2, NameCollisionOption.GenerateUniqueName);

            // Assert
            Assert.AreEqual(fileName1_renamed, file1.Name);
            var files = await folder.GetFilesAsync();
            Assert.IsFalse(files.Any(f => f.Name == fileName1));
            Assert.IsTrue(files.Any(f => f.Name == fileName2));
            Assert.IsTrue(files.Any(f => f.Name == fileName1_renamed));

            // Cleanup
            await file1.DeleteAsync();
            await file2.DeleteAsync();
        }

        [TestMethod]
        public async Task RenameFile_ReplaceExisting()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "fileToRename.txt";
            string renamedFile = "renamedFile.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);
            IFile existingFile = await folder.CreateFileAsync(renamedFile, CreationCollisionOption.FailIfExists);

            //	Act & Assert
            await file.RenameAsync(renamedFile, NameCollisionOption.ReplaceExisting);
            Assert.AreEqual(renamedFile, file.Name);

            var files = await folder.GetFilesAsync();
            Assert.IsTrue(files.Any(f => f.Name == renamedFile));
            Assert.IsFalse(files.Any(f => f.Name == originalFileName));

            // Cleanup
            await existingFile.DeleteAsync();
        }

        [TestMethod]
        public async Task RenameFile_BadArgs()
        {
            // Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "someFile.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);

            // Act & assert
            Task result = file.RenameAsync(string.Empty);
            Assert.IsTrue(result.IsFaulted);
            Assert.IsTrue(result.Exception.InnerException is ArgumentException);

            result = file.RenameAsync(null);
            Assert.IsTrue(result.IsFaulted);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());

            // Cleanup
            await file.DeleteAsync();
        }

        [TestMethod]
        public async Task MoveFile_WithinDirectory()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "fileToMove.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);

            //	Act
            string movedFile = "movedFile.txt";
            await file.MoveAsync(PortablePath.Combine(folder.Path, movedFile));

            //	Assert
            Assert.AreEqual(movedFile, file.Name);
            Assert.AreEqual(PortablePath.Combine(TestFileSystem.LocalStorage.Path, file.Name), file.Path);
            var files = await folder.GetFilesAsync();
            Assert.IsFalse(files.Any(f => f.Name == originalFileName));
            Assert.IsTrue(files.Any(f => f.Name == movedFile));

            // Cleanup
            await file.DeleteAsync();
        }

        [TestMethod]
        public async Task MoveFile_AcrossDirectories()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "fileToMove.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);
            var subfolder = await folder.CreateFolderAsync("subfolder", CreationCollisionOption.FailIfExists);

            //	Act
            string movedFile = "movedFile.txt";
            await file.MoveAsync(PortablePath.Combine(subfolder.Path, movedFile));

            //	Assert
            Assert.AreEqual(movedFile, file.Name);
            Assert.AreEqual(PortablePath.Combine(subfolder.Path, file.Name), file.Path);
            var files = await folder.GetFilesAsync();
            Assert.IsFalse(files.Any(f => f.Name == originalFileName));
            Assert.IsFalse(files.Any(f => f.Name == movedFile));
            files = await subfolder.GetFilesAsync();
            Assert.IsFalse(files.Any(f => f.Name == originalFileName));
            Assert.IsTrue(files.Any(f => f.Name == movedFile));

            // Cleanup
            await subfolder.DeleteAsync();
        }

        [TestMethod]
        public async Task MoveFile_FailIfExists()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "fileToMove.txt";
            string movedFile = "movedFile.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);
            IFile existingFile = await folder.CreateFileAsync(movedFile, CreationCollisionOption.FailIfExists);

            //	Act & Assert
            await ExceptionAssert.ThrowsAsync<IOException>(async () => await file.MoveAsync(PortablePath.Combine(folder.Path, movedFile), NameCollisionOption.FailIfExists));
            Assert.AreEqual(originalFileName, file.Name);

            var files = await folder.GetFilesAsync();
            Assert.IsTrue(files.Any(f => f.Name == movedFile));
            Assert.IsTrue(files.Any(f => f.Name == originalFileName));

            // Cleanup
            await file.DeleteAsync();
            await existingFile.DeleteAsync();
        }

        [TestMethod]
        public async Task MoveFile_GenerateUniqueName()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string fileName1 = "file1.txt";
            string fileName2 = "file2.txt";
            IFile file1 = await folder.CreateFileAsync(fileName1, CreationCollisionOption.FailIfExists);
            IFile file2 = await folder.CreateFileAsync(fileName2, CreationCollisionOption.FailIfExists);

            //	Act
            await file1.MoveAsync(PortablePath.Combine(folder.Path, fileName2), NameCollisionOption.GenerateUniqueName);

            // Assert
            string file1NameWithoutExtension = file1.Name.Substring(0, file1.Name.IndexOf('.'));
            string fileName2WithoutExtension = fileName2.Substring(0, fileName2.IndexOf('.'));
            Assert.IsTrue(file1NameWithoutExtension.StartsWith(fileName2WithoutExtension));
            Assert.IsFalse(file1NameWithoutExtension.Equals(fileName2WithoutExtension));
            var files = await folder.GetFilesAsync();
            Assert.IsFalse(files.Any(f => f.Name == fileName1));
            Assert.IsTrue(files.Any(f => f.Name == fileName2));
            Assert.IsTrue(files.Any(f => f.Name == file1.Name));

            // Cleanup
            await file1.DeleteAsync();
            await file2.DeleteAsync();
        }

        [TestMethod]
        public async Task MoveFile_ReplaceExisting()
        {
            //	Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "fileToMove.txt";
            string movedFile = "movedFile.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);
            IFile existingFile = await folder.CreateFileAsync(movedFile, CreationCollisionOption.FailIfExists);

            //	Act & Assert
            await file.MoveAsync(PortablePath.Combine(folder.Path, movedFile), NameCollisionOption.ReplaceExisting);
            Assert.AreEqual(movedFile, file.Name);

            var files = await folder.GetFilesAsync();
            Assert.IsTrue(files.Any(f => f.Name == movedFile));
            Assert.IsFalse(files.Any(f => f.Name == originalFileName));

            // Cleanup
            await existingFile.DeleteAsync();
        }

        [TestMethod]
        public async Task MoveFile_BadArgs()
        {
            // Arrange
            IFolder folder = TestFileSystem.LocalStorage;
            string originalFileName = "someFile.txt";
            IFile file = await folder.CreateFileAsync(originalFileName, CreationCollisionOption.FailIfExists);

            // Act & assert
            Task result = file.MoveAsync(string.Empty);
            Assert.IsTrue(result.IsFaulted);
            Assert.IsTrue(result.Exception.InnerException is ArgumentException);

            result = file.MoveAsync(null);
            Assert.IsTrue(result.IsFaulted);
            Assert.AreEqual(typeof(ArgumentNullException), result.Exception.InnerException.GetType());

            // Cleanup
            await file.DeleteAsync();
        }
    }
}
