using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCLStorage
{
    /// <summary>
    /// Represents a file in the <see cref="DesktopFileSystem"/>
    /// </summary>
    [DebuggerDisplay("Name = {_name}")]
    public class FileSystemFile : IFile
    {
        string _name;
        string _path;

        /// <summary>
        /// Creates a new <see cref="FileSystemFile"/> corresponding to the specified path
        /// </summary>
        /// <param name="path">The file path</param>
        public FileSystemFile(string path)
        {
            _name = System.IO.Path.GetFileName(path);
            _path = path;
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

            if (fileAccess == FileAccess.Read)
            {
                return File.OpenRead(Path);
            }
            else if (fileAccess == FileAccess.ReadAndWrite)
            {
                return File.Open(Path, FileMode.Open, System.IO.FileAccess.ReadWrite);
            }
            else
            {
                throw new ArgumentException("Unrecognized FileAccess value: " + fileAccess);
            }
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
            if (stream == null) return false;
            byte[] bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, (int)stream.Length).ConfigureAwait(false);
            File.WriteAllBytes(this.Path, bytes);
            stream.Close();
            return true;
        }

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <returns>A task which will complete after the file is deleted.</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);

            if (!File.Exists(Path))
            {
                throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + Path);
            }
            
            File.Delete(Path);
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

            await MoveAsync(PortablePath.Combine(System.IO.Path.GetDirectoryName(_path), newName), collisionOption, cancellationToken);
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

                if (File.Exists(candidatePath))
                {
                    switch (collisionOption)
                    {
                        case NameCollisionOption.FailIfExists:
                            throw new IOException("File already exists.");
                        case NameCollisionOption.GenerateUniqueName:
                            continue; // try again with a new name.
                        case NameCollisionOption.ReplaceExisting:
                            File.Delete(candidatePath);
                            break;
                    }
                }

                File.Move(_path, candidatePath);
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

                if (File.Exists(candidatePath))
                {
                    switch (collisionOption)
                    {
                        case NameCollisionOption.FailIfExists:
                            throw new IOException("File already exists.");
                        case NameCollisionOption.GenerateUniqueName:
                            continue; // try again with a new name.
                        case NameCollisionOption.ReplaceExisting:
                            File.Delete(candidatePath);
                            break;
                    }
                }

                File.Copy(_path, candidatePath);
                _path = candidatePath;
                _name = candidateName;
                return;
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
            //Extraction fails on Android if the zip files comes from the asset folder; couldnt' find out why
            var extractedFilenames = new List<string>();
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    ZipStorer zip = ZipStorer.Open(_path, System.IO.FileAccess.Read);
                    //// Read all directory contents
                    List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();
                    //// Extract all files in target directory
                    foreach (ZipStorer.ZipFileEntry entry in dir)
                    {
                        bool result = false;
                        var path = System.IO.Path.Combine(desinationFolder.Path, System.IO.Path.GetFileName(entry.FilenameInZip));
                        if (System.IO.File.Exists(path))
                        {
                            if (collisionOption == NameCollisionOption.ReplaceExisting)
                            {
                                System.IO.File.Delete(path);
                                result = zip.ExtractFile(entry, path);
                            }
                        }
                        else
                        {
                            result = zip.ExtractFile(entry, path);
                        }
                        if (result)
                        {
                            extractedFilenames.Add(entry.FilenameInZip);
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                    zip.Close();
                }
                catch (Exception)
                {

                }
            }, cancellationToken);
            return extractedFilenames;
        }

    }
}
