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
    [DebuggerDisplay("Name = {_name}")]
	public class IsoStoreFile : IFile
	{
		readonly string _name;
        readonly IsolatedStorageFile _root;
		readonly string _path;

        public IsoStoreFile(IsolatedStorageFile root, string path)
        {
            _root = root;
            _path = path;
            _name = System.IO.Path.GetFileName(path);
        }

		public IsoStoreFile(string name, IsoStoreFolder parentFolder)
            : this(parentFolder.Root, System.IO.Path.Combine(parentFolder.Path, name))
		{
			
		}

		public string Name
		{
			get { return _name; }
		}

		public string Path
		{
			get { return _path; }
		}

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
