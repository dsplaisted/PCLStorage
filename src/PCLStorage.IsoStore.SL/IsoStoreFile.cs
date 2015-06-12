using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

#if WINDOWS_PHONE
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace PCLStorage
{
    /// <summary>
    /// Represents a file in the <see cref="IsoStoreFileSystem"/>
    /// </summary>
    [DebuggerDisplay("Name = {_name}")]
    public class IsoStoreFile : IFile
    {
        readonly IsolatedStorageFile _root;
        string _name;
        string _path;

        /// <summary>
        /// Creates a new <see cref="IsoStoreFile"/> based on the path to it within an <see cref="IsolatedStorageFile"/>
        /// </summary>
        /// <param name="root">An <see cref="IsolatedStorageFile"/></param>
        /// <param name="path">The path in the <see cref="IsolatedStorageFile"/> to create an <see cref="IsoStoreFile"/> for</param>
        public IsoStoreFile(IsolatedStorageFile root, string path)
        {
            _root = root;
            _path = path;
            _name = System.IO.Path.GetFileName(path);
        }

        /// <summary>
        /// Creates a new <see cref="IsoStoreFile"/> from a file name and parent folder
        /// </summary>
        /// <param name="name">The file name</param>
        /// <param name="parentFolder">The parent folder</param>
        public IsoStoreFile(string name, IsoStoreFolder parentFolder)
            : this(parentFolder.Root, System.IO.Path.Combine(parentFolder.Path, name))
        {
            
        }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The "full path" of the file, which should uniquely identify it within a given <see cref="IFileSystem"/>
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Opens the file
        /// </summary>
        /// <param name="fileAccess">Specifies whether the file should be opened in read-only or read/write mode</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Stream"/> which can be used to read from or write to the file</returns>
        public async Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken)
        {
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            System.IO.FileAccess nativeFileAccess;
            if (fileAccess == FileAccess.Read)
            {
                nativeFileAccess = System.IO.FileAccess.Read;
            }
            else if (fileAccess == FileAccess.ReadAndWrite)
            {
                nativeFileAccess = System.IO.FileAccess.ReadWrite;
            }
            else
            {
                throw new ArgumentException("Unrecognized FileAccess value: " + fileAccess);
            }

            try
            {
                IsolatedStorageFileStream stream = _root.OpenFile(Path, FileMode.Open, nativeFileAccess, FileShare.Read);
                return stream;
            }
            catch (IsolatedStorageException ex)
            {
                //	Check to see if error is because file does not exist, if so throw a more specific exception
                bool fileDoesntExist = false;
                try
                {
                    if (!_root.FileExists(Path))
                    {
                        fileDoesntExist = true;
                    }
                }
                catch { }
                if (fileDoesntExist)
                {
                    throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + Path, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        public Task<bool> WriteAsync(Stream stream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <returns>A task which will complete after the file is deleted.</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            try
            {
#if WINDOWS_PHONE
                //  Windows Phone (at least WP7) doesn't throw an error if you try to delete something that doesn't exist,
                //  so check for this manually for consistent behavior across platforms
                if (!_root.FileExists(Path))
                {
                    throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + Path);
                }
#endif
                _root.DeleteFile(Path);
            }
            catch (IsolatedStorageException ex)
            {
                //	Check to see if error is because file does not exist, if so throw a more specific exception
                bool fileDoesntExist = false;
                try
                {
                    if (!_root.FileExists(Path))
                    {
                        fileDoesntExist = true;
                    }
                }
                catch { }
                if (fileDoesntExist)
                {
                    throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + Path, ex);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Renames a file without changing its location.
        /// </summary>
        /// <param name="newName">The new leaf name of the file.</param>
        /// <param name="collisionOption">How to deal with collisions with existing files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task which will complete after the file is renamed.
        /// </returns>
        public async Task RenameAsync(string newName, NameCollisionOption collisionOption, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(newName, "newName");

            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            string newPath = PortablePath.Combine(System.IO.Path.GetDirectoryName(_path), newName);
            await MoveAsync(newPath, collisionOption, cancellationToken);
        }

        /// <summary>
        /// Moves a file.
        /// </summary>
        /// <param name="newPath">The new full path of the file.</param>
        /// <param name="collisionOption">How to deal with collisions with existing files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task which will complete after the file is moved.
        /// </returns>
        public async Task MoveAsync(string newPath, NameCollisionOption collisionOption, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(newPath, "newPath");

            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            string newDirectory = System.IO.Path.GetDirectoryName(newPath);
            string newName = System.IO.Path.GetFileName(newPath);

            for (int counter = 1; ; counter++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string candidateName = newName;
                if (counter > 1)
                {
                    candidateName = String.Format(
                        CultureInfo.InvariantCulture,
                        "{0} ({1}){2}",
                        System.IO.Path.GetFileNameWithoutExtension(newName),
                        counter,
                        System.IO.Path.GetExtension(newName));
                }

                string candidatePath = PortablePath.Combine(newDirectory, candidateName);

                if (_root.FileExists(candidatePath))
                {
                    switch (collisionOption)
                    {
                        case NameCollisionOption.FailIfExists:
                            throw new IOException("File already exists.");
                        case NameCollisionOption.GenerateUniqueName:
                            continue; // try again with a new name.
                        case NameCollisionOption.ReplaceExisting:
                            _root.DeleteFile(candidatePath);
                            break;
                    }
                }

                _root.MoveFile(_path, candidatePath);
                _path = candidatePath;
                _name = candidateName;
                return;
            }
        }

        /// <summary>
        /// Copy a file.
        /// </summary>
        /// <param name="newPath">The new full path of the file.</param>
        /// <param name="collisionOption">How to deal with collisions with existing files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which will complete after the file is moved.</returns>
        public async Task CopyAsync(string newPath, NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting, CancellationToken cancellationToken = default(CancellationToken))
        {
            Requires.NotNullOrEmpty(newPath, "newPath");

            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);
            string newDirectory = System.IO.Path.GetDirectoryName(newPath);
            string newName = System.IO.Path.GetFileName(newPath);

            for (int counter = 1; ; counter++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string candidateName = newName;
                if (counter > 1)
                {
                    candidateName = String.Format(
                        CultureInfo.InvariantCulture,
                        "{0} ({1}){2}",
                        System.IO.Path.GetFileNameWithoutExtension(newName),
                        counter,
                        System.IO.Path.GetExtension(newName));
                }

                string candidatePath = PortablePath.Combine(newDirectory, candidateName);

                if (_root.FileExists(candidatePath))
                {
                    switch (collisionOption)
                    {
                        case NameCollisionOption.FailIfExists:
                            throw new IOException("File already exists.");
                        case NameCollisionOption.GenerateUniqueName:
                            continue; // try again with a new name.
                        case NameCollisionOption.ReplaceExisting:
                            _root.DeleteFile(candidatePath);
                            break;
                    }
                }

                _root.CopyFile(_path, candidatePath);
                _path = candidatePath;
                _name = candidateName;
                return;
            }
        }

        public Task<System.Collections.Generic.List<string>> ExtractZipAsync(IFolder desinationFolder, NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

    }
}
