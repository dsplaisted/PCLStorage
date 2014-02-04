using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    /// <summary>
    /// Represents a folder in the <see cref="DesktopFileSystem"/>
    /// </summary>
    [DebuggerDisplay("Name = {_name}")]
    public class FileSystemFolder : IFolder
    {
        readonly string _name;
        readonly string _path;
        readonly bool _canDelete;

        /// <summary>
        /// Creates a new <see cref="FileSystemFolder" /> corresponding to a specified path
        /// </summary>
        /// <param name="path">The folder path</param>
        /// <param name="canDelete">Specifies whether the folder can be deleted (via <see cref="DeleteAsync"/>)</param>
        public FileSystemFolder(string path, bool canDelete)
        {
            _name = System.IO.Path.GetFileName(path);
            _path = path;
            _canDelete = canDelete;
        }

        /// <summary>
        /// Creates a new <see cref="FileSystemFolder" /> corresponding to a specified path
        /// </summary>
        /// <param name="path">The folder path</param>
        /// <remarks>A folder created with this constructor cannot be deleted via <see cref="DeleteAsync"/></remarks>
        public FileSystemFolder(string path)
            : this(path, false)
        {
        }

        /// <summary>
        /// The name of the folder
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The "full path" of the folder, which should uniquely identify it within a given <see cref="IFileSystem"/>
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Creates a file in this folder
        /// </summary>
        /// <param name="desiredName">The name of the file to create</param>
        /// <param name="option">Specifies how to behave if the specified file already exists</param>
        /// <returns>The newly created file</returns>
        public Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option)
        {
            EnsureExists();

            string nameToUse = desiredName;
            string newPath = System.IO.Path.Combine(Path, nameToUse);
            if (File.Exists(newPath))
            {
                if (option == CreationCollisionOption.GenerateUniqueName)
                {
                    string desiredRoot = System.IO.Path.GetFileNameWithoutExtension(desiredName);
                    string desiredExtension = System.IO.Path.GetExtension(desiredName);
                    for (int num = 2; File.Exists(newPath); num++)
                    {
                        nameToUse = desiredRoot + " (" + num + ")" + desiredExtension;
                        newPath = System.IO.Path.Combine(Path, nameToUse);
                    }
                    InternalCreateFile(newPath);
                }
                else if (option == CreationCollisionOption.ReplaceExisting)
                {
                    File.Delete(newPath);
                    InternalCreateFile(newPath);
                }
                else if (option == CreationCollisionOption.FailIfExists)
                {
                    throw new IOException("File already exists: " + newPath);
                }
                else if (option == CreationCollisionOption.OpenIfExists)
                {
                    //	No operation
                }
                else
                {
                    throw new ArgumentException("Unrecognized CreationCollisionOption: " + option);
                }
            }
            else
            {
                //	Create file
                InternalCreateFile(newPath);
            }

            var ret = new FileSystemFile(newPath);
            return Task.FromResult<IFile>(ret);
        }

        void InternalCreateFile(string path)
        {
            using (var stream = File.Create(path))
            {
            }

        }

        /// <summary>
        /// Gets a file in this folder
        /// </summary>
        /// <param name="name">The name of the file to get</param>
        /// <returns>The requested file, or null if it does not exist</returns>
        public Task<IFile> GetFileAsync(string name)
        {
            string path = System.IO.Path.Combine(Path, name);
            if (!File.Exists(path))
            {
                throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + path);
            }
            var ret = new FileSystemFile(path);
            return Task.FromResult<IFile>(ret);
        }

        /// <summary>
        /// Gets a list of the files in this folder
        /// </summary>
        /// <returns>A list of the files in the folder</returns>
        public Task<IList<IFile>> GetFilesAsync()
        {
            EnsureExists();
            IList<IFile> ret = Directory.GetFiles(Path).Select(f => new FileSystemFile(f)).ToList<IFile>().AsReadOnly();
            return Task.FromResult(ret);
        }

        /// <summary>
        /// Creates a subfolder in this folder
        /// </summary>
        /// <param name="desiredName">The name of the folder to create</param>
        /// <param name="option">Specifies how to behave if the specified folder already exists</param>
        /// <returns>The newly created folder</returns>
        public Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option)
        {
            EnsureExists();
            string nameToUse = desiredName;
            string newPath = System.IO.Path.Combine(Path, nameToUse);
            if (Directory.Exists(newPath))
            {
                if (option == CreationCollisionOption.GenerateUniqueName)
                {
                    for (int num = 2; Directory.Exists(newPath); num++)
                    {
                        nameToUse = desiredName + " (" + num + ")";
                        newPath = System.IO.Path.Combine(Path, nameToUse);
                    }
                    Directory.CreateDirectory(newPath);
                }
                else if (option == CreationCollisionOption.ReplaceExisting)
                {
                    Directory.Delete(newPath, true);
                    Directory.CreateDirectory(newPath);
                }
                else if (option == CreationCollisionOption.FailIfExists)
                {
                    throw new IOException("Directory already exists: " + newPath);
                }
                else if (option == CreationCollisionOption.OpenIfExists)
                {
                    //	No operation
                }
                else
                {
                    throw new ArgumentException("Unrecognized CreationCollisionOption: " + option);
                }
            }
            else
            {
                Directory.CreateDirectory(newPath);
            }

            var ret = new FileSystemFolder(newPath, true);
            return Task.FromResult<IFolder>(ret);
        }

        /// <summary>
        /// Gets a subfolder in this folder
        /// </summary>
        /// <param name="name">The name of the folder to get</param>
        /// <returns>The requested folder, or null if it does not exist</returns>
        public Task<IFolder> GetFolderAsync(string name)
        {
            string path = System.IO.Path.Combine(Path, name);
            if (!Directory.Exists(path))
            {
                throw new PCLStorage.Exceptions.DirectoryNotFoundException("Directory does not exist: " + path);
            }
            var ret = new FileSystemFolder(path, true);
            return Task.FromResult<IFolder>(ret);
        }

        /// <summary>
        /// Gets a list of subfolders in this folder
        /// </summary>
        /// <returns>A list of subfolders in the folder</returns>
        public Task<IList<IFolder>> GetFoldersAsync()
        {
            EnsureExists();
            IList<IFolder> ret = Directory.GetDirectories(Path).Select(d => new FileSystemFolder(d, true)).ToList<IFolder>().AsReadOnly();
            return Task.FromResult(ret);
        }

        /// <summary>
        /// Checks whether a folder or file exists at the given location.
        /// </summary>
        /// <param name="name">The name of the file or folder to check for.</param>
        /// <returns>
        /// A task whose result is the result of the existence check.
        /// </returns>
        public Task<ExistenceCheckResult> CheckExistsAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException();
            }

            string checkPath = PortablePath.Combine(this.Path, name);
            if (File.Exists(checkPath))
            {
                return Task.FromResult(ExistenceCheckResult.FileExists);
            }
            else if (Directory.Exists(checkPath))
            {
                return Task.FromResult(ExistenceCheckResult.FolderExists);
            }
            else
            {
                return Task.FromResult(ExistenceCheckResult.NotFound);
            }
        }

        /// <summary>
        /// Deletes this folder and all of its contents
        /// </summary>
        /// <returns>A task which will complete after the folder is deleted</returns>
        public Task DeleteAsync()
        {
            if (!_canDelete)
            {
                throw new IOException("Cannot delete root storage folder.");
            }
            EnsureExists();
            Directory.Delete(Path, true);
            return Task.FromResult(true);
        }

        void EnsureExists()
        {
            if (!Directory.Exists(Path))
            {
                throw new PCLStorage.Exceptions.DirectoryNotFoundException("Directory does not exist: " + Path);
            }
        }
    }
}
