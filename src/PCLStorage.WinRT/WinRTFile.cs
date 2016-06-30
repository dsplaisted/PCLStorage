using System;
using System.Collections.Generic;
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
    /// Represents a file in the <see cref="WinRTFileSystem"/>
    /// </summary>
    [DebuggerDisplay("Name = {Name}")]
    public class WinRTFile : IFile
    {
        /// <summary>
        /// The HRESULT on a System.Exception thrown when a file collision occurs.
        /// </summary>
        internal const int FILE_ALREADY_EXISTS = unchecked((int)0x800700B7);

        private readonly IStorageFile _wrappedFile;

        /// <summary>
        /// Creates a new <see cref="WinRTFile"/>
        /// </summary>
        /// <param name="wrappedFile">The WinRT <see cref="IStorageFile"/> to wrap</param>
        public WinRTFile(IStorageFile wrappedFile)
        {
            _wrappedFile = wrappedFile;
        }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string Name
        {
            get { return _wrappedFile.Name; }
        }

        /// <summary>
        /// The "full path" of the file, which should uniquely identify it within a given <see cref="IFileSystem"/>
        /// </summary>
        public string Path
        {
            get { return _wrappedFile.Path; }
        }

        /// <summary>
        /// Opens the file
        /// </summary>
        /// <param name="fileAccess">Specifies whether the file should be opened in read-only or read/write mode</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Stream"/> which can be used to read from or write to the file</returns>
        public async Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken)
        {
            FileAccessMode fileAccessMode;
            if (fileAccess == FileAccess.Read)
            {
                fileAccessMode = FileAccessMode.Read;
            }
            else if (fileAccess == FileAccess.ReadAndWrite)
            {
                fileAccessMode = FileAccessMode.ReadWrite;
            }
            else
            {
                throw new ArgumentException("Unrecognized FileAccess value: " + fileAccess);
            }

            var wrtStream = await _wrappedFile.OpenAsync(fileAccessMode).AsTask(cancellationToken).ConfigureAwait(false);
            return wrtStream.AsStream();
        }

        /// <summary>
        /// Writes a stream to the file
        /// </summary>
        /// <param name="stream">The data stream which should be written to the file.</param>
        /// <param name="fileAccess">Specifies whether the file should be overridden.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="bool"/> returns true for success</returns>
        public async Task<bool> WriteAsync(Stream stream, CancellationToken cancellationToken)
        {
            return await TargetPlatformFileSystem.WriteStreamAsync(this, stream, cancellationToken);
        }

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <returns>A task which will complete after the file is deleted.</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _wrappedFile.DeleteAsync().AsTask(cancellationToken).ConfigureAwait(false);
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

            try
            {
                await _wrappedFile.RenameAsync(newName, (Windows.Storage.NameCollisionOption)collisionOption).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex.HResult == FILE_ALREADY_EXISTS)
                {
                    throw new IOException("File already exists.", ex);
                }

                throw;
            }
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

            var newFolder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(newPath)).AsTask(cancellationToken).ConfigureAwait(false);
            string newName = System.IO.Path.GetFileName(newPath);

            try
            {
                await _wrappedFile.MoveAsync(newFolder, newName, (Windows.Storage.NameCollisionOption)collisionOption).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex.HResult == FILE_ALREADY_EXISTS)
                {
                    throw new IOException("File already exists.", ex);
                }

                throw;
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

            var newFolder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(newPath)).AsTask(cancellationToken).ConfigureAwait(false);
            string newName = System.IO.Path.GetFileName(newPath);

            try
            {
                await _wrappedFile.CopyAsync(newFolder, newName, (Windows.Storage.NameCollisionOption)collisionOption).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex.HResult == FILE_ALREADY_EXISTS)
                {
                    throw new IOException("File already exists.", ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Extract a zip file.
        /// </summary>
        /// <param name="desinationFolder">The destination folder for zip file extraction</param>
        /// <param name="collisionOption">How to deal with collisions with existing files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task with a List of strings containing the names of extracted files from the zip archive.</returns>
        public async Task<List<string>> ExtractZipAsync(IFolder desinationFolder, NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await TargetPlatformFileSystem.ExtractZipAsync(this, desinationFolder, collisionOption, cancellationToken);
        }

    }
}
