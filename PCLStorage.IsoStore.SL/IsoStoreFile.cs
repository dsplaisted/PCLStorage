using System;
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
	public class IsoStoreFile : IFile
	{
		readonly string _name;
		readonly IsoStoreFolder _parentFolder;
		readonly string _path;

		public IsoStoreFile(string name, IsoStoreFolder parentFolder)
		{
			_name = name;
			_parentFolder = parentFolder;
			_path = System.IO.Path.Combine(_parentFolder.Path, _name);

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
				IsolatedStorageFileStream stream = _parentFolder.Root.OpenFile(Path, FileMode.Open, nativeFileAccess, FileShare.Read);
				return TaskEx.FromResult<Stream>(stream);
			}
			catch (IsolatedStorageException ex)
			{
				//	Check to see if error is because file does not exist, if so throw a more specific exception
				bool fileDoesntExist = false;
				try
				{
					if (!_parentFolder.Root.FileExists(Path))
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
                _parentFolder.Root.DeleteFile(Path);
            }
            catch (IsolatedStorageException ex)
            {
                //	Check to see if error is because file does not exist, if so throw a more specific exception
                bool fileDoesntExist = false;
                try
                {
                    if (!_parentFolder.Root.FileExists(Path))
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
