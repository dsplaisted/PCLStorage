using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Threading;

#if WINDOWS_PHONE
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace PCLStorage
{
    /// <summary>
    /// Represents a folder in the <see cref="IsoStoreFileSystem"/>
    /// </summary>
    [DebuggerDisplay("Name = {_name}")]
    public class IsoStoreFolder : IFolder
    {
        internal IsolatedStorageFile Root { get; private set; }

        readonly string _name;
        readonly string _path;

        /// <summary>
        /// Creates a new <see cref="IsoStoreFolder"/> corresponding to the specified <see cref="IsolatedStorageFile"/>
        /// </summary>
        /// <param name="root">An <see cref="IsolatedStorageFile"/> to create an <see cref="IsoStoreFolder"/> for</param>
        public IsoStoreFolder(IsolatedStorageFile root)
        {
            Root = root;
            _name = string.Empty;
            _path = string.Empty;
        }

        /// <summary>
        /// Creates a new <see cref="IsoStoreFolder"/> corresponding to the specified path in an <see cref="IsolatedStorageFile"/>
        /// </summary>
        /// <param name="root">An <see cref="IsolatedStorageFile"/></param>
        /// <param name="path">The path in the <see cref="IsolatedStorageFile"/> to create an <see cref="IsoStoreFolder"/> for</param>
        public IsoStoreFolder(IsolatedStorageFile root, string path)
        {
            Root = root;
            //  Trim trailing backslash / slash off of end if it exists
            if (path.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                path = path.Substring(0, path.Length - 1);
            }
            _name = System.IO.Path.GetFileName(path);
            _path = path;
        }

        /// <summary>
        /// Creates a new <see cref="IsoStoreFolder"/> corresponding to a subfolder of another <see cref="IsoStoreFolder"/>
        /// </summary>
        /// <param name="name">The name of the folder</param>
        /// <param name="parent">The parent folder</param>
        public IsoStoreFolder(string name, IsoStoreFolder parent)
            : this(parent.Root, System.IO.Path.Combine(parent.Path, name))
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly created file</returns>
        public async Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(desiredName, "desiredName");
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            EnsureExists();
            string nameToUse = desiredName;
            string path = System.IO.Path.Combine(Path, nameToUse);
            if (Root.FileExists(path))
            {
                if (option == CreationCollisionOption.GenerateUniqueName)
                {
                    string desiredRoot = System.IO.Path.GetFileNameWithoutExtension(desiredName);
                    string desiredExtension = System.IO.Path.GetExtension(desiredName);
                    for (int num = 2; Root.FileExists(path) ; num++)
                    {
                        nameToUse = desiredRoot + " (" + num + ")" + desiredExtension;
                        path = System.IO.Path.Combine(Path, nameToUse);
                    }
                    InternalCreateFile(path);
                }
                else if (option == CreationCollisionOption.ReplaceExisting)
                {
                    Root.DeleteFile(path);
                    InternalCreateFile(path);
                }
                else if (option == CreationCollisionOption.FailIfExists)
                {
                    throw new IOException("File already exists: " + path);
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
                InternalCreateFile(path);
            }

            var ret = new IsoStoreFile(nameToUse, this);
            return ret;
        }

        void InternalCreateFile(string path)
        {
            using (var stream = new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, Root))
            {
            }
        }

        /// <summary>
        /// Gets a file in this folder
        /// </summary>
        /// <param name="name">The name of the file to get</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested file, or null if it does not exist</returns>
        public async Task<IFile> GetFileAsync(string name, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(name, "name");
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            EnsureExists();

            string path = System.IO.Path.Combine(Path, name);
            if (!Root.FileExists(path))
            {
                throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + path);
            }
            var ret = new IsoStoreFile(name, this);
            return ret;
        }

        /// <summary>
        /// Gets a list of the files in this folder
        /// </summary>
        /// <returns>A list of the files in the folder</returns>
        public async Task<IList<IFile>> GetFilesAsync(CancellationToken cancellationToken)
        {
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            EnsureExists();

            string[] fileNames = Root.GetFileNames(System.IO.Path.Combine(Path, "*.*"));
            IList<IFile> ret = fileNames.Select(fn => new IsoStoreFile(fn, this)).Cast<IFile>().ToList().AsReadOnly();
            return ret;
        }

        /// <summary>
        /// Creates a subfolder in this folder
        /// </summary>
        /// <param name="desiredName">The name of the folder to create</param>
        /// <param name="option">Specifies how to behave if the specified folder already exists</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly created folder</returns>
        public async Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(desiredName, "desiredName");

            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            EnsureExists();

            string nameToUse = desiredName;
            string newPath = System.IO.Path.Combine(Path, nameToUse);
            if (Root.DirectoryExists(newPath))
            {
                if (option == CreationCollisionOption.GenerateUniqueName)
                {
                    for (int num = 2; Root.DirectoryExists(newPath); num++)
                    {
                        nameToUse = desiredName + " (" + num + ")";
                        newPath = System.IO.Path.Combine(Path, nameToUse);
                    }
                    Root.CreateDirectory(newPath);
                }
                else if (option == CreationCollisionOption.ReplaceExisting)
                {
                    IsoStoreFolder folderToDelete = new IsoStoreFolder(nameToUse, this);
                    await folderToDelete.DeleteAsync(cancellationToken).ConfigureAwait(false);
                    Root.CreateDirectory(newPath);
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
                Root.CreateDirectory(newPath);
            }

            var ret = new IsoStoreFolder(nameToUse, this);
            return ret;
        }

        /// <summary>
        /// Gets a subfolder in this folder
        /// </summary>
        /// <param name="name">The name of the folder to get</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The requested folder, or null if it does not exist</returns>
        public async Task<IFolder> GetFolderAsync(string name, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(name, "name");
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            EnsureExists();

            string path = System.IO.Path.Combine(Path, name);
            if (!Root.DirectoryExists(path))
            {
                throw new PCLStorage.Exceptions.DirectoryNotFoundException("Directory does not exist: " + path);
            }
            var ret = new IsoStoreFolder(name, this);
            return ret;
        }

        /// <summary>
        /// Gets a list of subfolders in this folder
        /// </summary>
        /// <returns>A list of subfolders in the folder</returns>
        public async Task<IList<IFolder>> GetFoldersAsync(CancellationToken cancellationToken)
        {
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            EnsureExists();

            string[] folderNames = Root.GetDirectoryNames(System.IO.Path.Combine(Path, "*"));
            IList<IFolder> ret = folderNames.Select(fn => new IsoStoreFolder(fn, this)).Cast<IFolder>().ToList().AsReadOnly();
            return ret;
        }

        /// <summary>
        /// Checks whether a folder or file exists at the given location.
        /// </summary>
        /// <param name="name">The name of the file or folder to check for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task whose result is the result of the existence check.
        /// </returns>
        public async Task<ExistenceCheckResult> CheckExistsAsync(string name, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(name, "name");

            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            var checkPath = PortablePath.Combine(Path, name);
            if (Root.FileExists(checkPath))
            {
                return ExistenceCheckResult.FileExists;
            }
            else if (Root.DirectoryExists(checkPath))
            {
                return ExistenceCheckResult.FolderExists;
            }
            else
            {
                return ExistenceCheckResult.NotFound;
            }
        }

        /// <summary>
        /// Deletes this folder and all of its contents
        /// </summary>
        /// <returns>A task which will complete after the folder is deleted</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            EnsureExists();

            if (string.IsNullOrEmpty(Path))
            {
                throw new IOException("Cannot delete root Isolated Storage folder.");
            }

            foreach (var subfolder in await GetFoldersAsync(cancellationToken).ConfigureAwait(false))
            {
                await subfolder.DeleteAsync(cancellationToken).ConfigureAwait(false);
            }

            foreach (var file in await GetFilesAsync(cancellationToken).ConfigureAwait(false))
            {
                await file.DeleteAsync(cancellationToken).ConfigureAwait(false);
            }

            Root.DeleteDirectory(Path);
        }

        void EnsureExists()
        {
            if (!Root.DirectoryExists(Path))
            {
                throw new Exceptions.DirectoryNotFoundException("The specified folder does not exist: " + Path);
            }
        }
    }
}
