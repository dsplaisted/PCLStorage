using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        /// <returns>A <see cref="Stream"/> which can be used to read from or write to the file</returns>
        public Task<Stream> OpenAsync(FileAccess fileAccess)
        {
            Stream ret;
            if (fileAccess == FileAccess.Read)
            {
                ret = File.OpenRead(Path);
            }
            else if (fileAccess == FileAccess.ReadAndWrite)
            {
                ret = File.Open(Path, FileMode.Open, System.IO.FileAccess.ReadWrite);
            }
            else
            {
                throw new ArgumentException("Unrecognized FileAccess value: " + fileAccess);
            }
            return Task.FromResult(ret);
        }

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <returns>A task which will complete after the file is deleted.</returns>
        public Task DeleteAsync()
        {
            if (!File.Exists(Path))
            {
                throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + Path);
            }
            File.Delete(Path);

            return Task.FromResult(true);
        }

        public Task RenameAsync(string newName, NameCollisionOption collisionOption)
        {
            return MoveAsync(PortablePath.Combine(System.IO.Path.GetDirectoryName(_path), newName), collisionOption);
        }

        public Task MoveAsync(string newPath, NameCollisionOption collisionOption)
        {
            if (newPath == null)
            {
                throw new ArgumentNullException("newPath");
            }
            else if (newPath.Length == 0)
            {
                throw new ArgumentException();
            }

            string newDirectory = System.IO.Path.GetDirectoryName(newPath);
            string newName = System.IO.Path.GetFileName(newPath);

            for (int counter = 0; ; counter++)
            {
                string candidateName = newName;
                if (counter > 0)
                {
                    candidateName = String.Format(
                        CultureInfo.InvariantCulture,
                        "{0} ({1}){2}",
                        System.IO.Path.GetFileNameWithoutExtension(newName),
                        counter,
                        System.IO.Path.GetExtension(newName));
                }

                string candidatePath = PortablePath.Combine(newDirectory, candidateName);

                switch (collisionOption)
                {
                    case NameCollisionOption.FailIfExists:
                        if (File.Exists(candidatePath))
                        {
                            throw new IOException("File already exists.");
                        }

                        break;
                    case NameCollisionOption.GenerateUniqueName:
                        if (File.Exists(candidatePath))
                        {
                            // try again with the new name.
                            continue;
                        }

                        break;
                }

                File.Move(_path, candidatePath);
                _path = candidatePath;
                _name = candidateName;
                return Task.FromResult(true);
            }
        }
    }
}
