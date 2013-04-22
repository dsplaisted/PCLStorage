using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.IO;

namespace PCLStorage
{
    /// <summary>
    /// Represents a file in the <see cref="IsoStoreFileSystem"/>
    /// </summary>
    [DebuggerDisplay("Name = {_name}")]
	public class IsoStoreFile : IFile
	{
		readonly string _name;
        readonly IsolatedStorageFile _root;
		readonly string _path;

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
        /// <returns>A <see cref="Stream"/> which can be used to read from or write to the file</returns>
		public Task<Stream> OpenAsync(FileAccess fileAccess)
		{
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
				return TaskEx.FromResult<Stream>(stream);
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
        /// Deletes the file
        /// </summary>
        /// <returns>A task which will complete after the file is deleted.</returns>
		public Task DeleteAsync()
		{
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
			return TaskEx.FromResult(true);
		}
	}
}
