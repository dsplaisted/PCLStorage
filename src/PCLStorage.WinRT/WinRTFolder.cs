using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PCLStorage
{
    /// <summary>
    /// Represents a folder in the <see cref="WinRTFileSystem"/>
    /// </summary>
    [DebuggerDisplay("Name = {Name}")]
    public class WinRTFolder : IFolder
    {
        private readonly IStorageFolder _wrappedFolder;
        private readonly bool _isRootFolder;

        /// <summary>
        /// Creates a new <see cref="WinRTFolder"/>
        /// </summary>
        /// <param name="wrappedFolder">The WinRT <see cref="IStorageFolder"/> to wrap</param>
        public WinRTFolder(IStorageFolder wrappedFolder)
        {
            _wrappedFolder = wrappedFolder;
            if (_wrappedFolder.Path == Windows.Storage.ApplicationData.Current.LocalFolder.Path
#if !WINDOWS_PHONE
 || _wrappedFolder.Path == Windows.Storage.ApplicationData.Current.RoamingFolder.Path
#endif
)
            {
                _isRootFolder = true;
            }
            else
            {
                _isRootFolder = false;
            }
        }

        /// <summary>
        /// The name of the folder
        /// </summary>
        public string Name
        {
            get { return _wrappedFolder.Name; }
        }

        /// <summary>
        /// The "full path" of the folder, which should uniquely identify it within a given <see cref="IFileSystem"/>
        /// </summary>
        public string Path
        {
            get { return _wrappedFolder.Path; }
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
            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);
            StorageFile wrtFile;
            try
            {
                wrtFile = await _wrappedFolder.CreateFileAsync(desiredName, GetWinRTCreationCollisionOption(option)).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex.HResult == WinRTFile.FILE_ALREADY_EXISTS)
                {
                    //  File already exists (and potentially other failures, not sure what the HResult represents)
                    throw new IOException(ex.Message, ex);
                }
                throw;
            }
            return new WinRTFile(wrtFile);
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

            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var wrtFile = await _wrappedFolder.GetFileAsync(name).AsTask(cancellationToken).ConfigureAwait(false);
                return new WinRTFile(wrtFile);
            }
            catch (FileNotFoundException ex)
            {
                throw new Exceptions.FileNotFoundException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets a list of the files in this folder
        /// </summary>
        /// <returns>A list of the files in the folder</returns>
        public async Task<IList<IFile>> GetFilesAsync(CancellationToken cancellationToken)
        {
            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);
            var wrtFiles = await _wrappedFolder.GetFilesAsync().AsTask(cancellationToken).ConfigureAwait(false);
            var files = wrtFiles.Select(f => new WinRTFile(f)).ToList<IFile>();
            return new ReadOnlyCollection<IFile>(files);
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
            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);
            StorageFolder wrtFolder;
            try
            {
                wrtFolder = await _wrappedFolder.CreateFolderAsync(desiredName, GetWinRTCreationCollisionOption(option)).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex.HResult == WinRTFile.FILE_ALREADY_EXISTS)
                {
                    //  Folder already exists (and potentially other failures, not sure what the HResult represents)
                    throw new IOException(ex.Message, ex);
                }
                throw;
            }
            return new WinRTFolder(wrtFolder);
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

            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);
            StorageFolder wrtFolder;
            try
            {
                wrtFolder = await _wrappedFolder.GetFolderAsync(name).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException ex)
            {
                //  Folder does not exist
                throw new Exceptions.DirectoryNotFoundException(ex.Message, ex);
            }
            return new WinRTFolder(wrtFolder);
        }

        /// <summary>
        /// Gets a list of subfolders in this folder
        /// </summary>
        /// <returns>A list of subfolders in the folder</returns>
        public async Task<IList<IFolder>> GetFoldersAsync(CancellationToken cancellationToken)
        {
            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);
            var wrtFolders = await _wrappedFolder.GetFoldersAsync().AsTask(cancellationToken).ConfigureAwait(false);
            var folders = wrtFolders.Select(f => new WinRTFolder(f)).ToList<IFolder>();
            return new ReadOnlyCollection<IFolder>(folders);
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

            // WinRT does not expose an Exists method, so we have to
            // try accessing the entity to see if it succeeds.
            // We could code this up with a catch block, but that means
            // that a file existence check requires first chance exceptions
            // are thrown and caught, which *can* slow the app down,
            // and also bugs the developer who is debugging the app.
            // So we just avoid all exceptions being *thrown*
            // by checking for exception objects carefully.
            var result = await _wrappedFolder.GetItemAsync(name).AsTaskNoThrow(cancellationToken);
            if (result.IsFaulted)
            {
                if (result.Exception.InnerException is FileNotFoundException)
                {
                    return ExistenceCheckResult.NotFound;
                }
                else
                {
                    // rethrow unexpected exceptions.
                    result.GetAwaiter().GetResult();
                    throw result.Exception; // shouldn't reach here anyway.
                }
            }
            else if (result.IsCanceled)
            {
                throw new OperationCanceledException();
            }
            else
            {
                IStorageItem storageItem = result.Result;
                if (storageItem.IsOfType(StorageItemTypes.File))
                {
                    return ExistenceCheckResult.FileExists;
                }
                else if (storageItem.IsOfType(StorageItemTypes.Folder))
                {
                    return ExistenceCheckResult.FolderExists;
                }
                else
                {
                    return ExistenceCheckResult.NotFound;
                }
            }
        }

        /// <summary>
        /// Deletes this folder and all of its contents
        /// </summary>
        /// <returns>A task which will complete after the folder is deleted</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            await EnsureExistsAsync(cancellationToken).ConfigureAwait(false);

            if (_isRootFolder)
            {
                throw new IOException("Cannot delete root storage folder.");
            }

            await _wrappedFolder.DeleteAsync().AsTask(cancellationToken).ConfigureAwait(false);
        }

        Windows.Storage.CreationCollisionOption GetWinRTCreationCollisionOption(CreationCollisionOption option)
        {
            if (option == CreationCollisionOption.GenerateUniqueName)
            {
                return Windows.Storage.CreationCollisionOption.GenerateUniqueName;
            }
            else if (option == CreationCollisionOption.ReplaceExisting)
            {
                return Windows.Storage.CreationCollisionOption.ReplaceExisting;
            }
            else if (option == CreationCollisionOption.FailIfExists)
            {
                return Windows.Storage.CreationCollisionOption.FailIfExists;
            }
            else if (option == CreationCollisionOption.OpenIfExists)
            {
                return Windows.Storage.CreationCollisionOption.OpenIfExists;
            }
            else
            {
                throw new ArgumentException("Unrecognized CreationCollisionOption value: " + option);
            }
        }

        async Task EnsureExistsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await StorageFolder.GetFolderFromPathAsync(Path).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException ex)
            {
                //  Folder does not exist
                throw new Exceptions.DirectoryNotFoundException(ex.Message, ex);
            }
        }
    }
}
