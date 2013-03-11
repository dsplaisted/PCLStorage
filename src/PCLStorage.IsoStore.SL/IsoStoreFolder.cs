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

namespace PCLStorage
{
    [DebuggerDisplay("Name = {_name}")]
	public class IsoStoreFolder : IFolder
	{
		internal IsolatedStorageFile Root { get; private set; }

		readonly string _name;
		readonly string _path;

		public IsoStoreFolder()
			: this (IsolatedStorageFile.GetUserStoreForApplication())
		{
		}

		public IsoStoreFolder(IsolatedStorageFile root)
		{
			Root = root;
			_name = string.Empty;
			_path = string.Empty;
		}

		public IsoStoreFolder(string name, IsoStoreFolder parent)
		{
			Root = parent.Root;
			_name = name;
			_path = System.IO.Path.Combine(parent.Path, _name);
		}

		public string Name
		{
			get { return _name; }
		}

		public string Path
		{
			get { return _path; }
		}

		public Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option)
		{
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
			return TaskEx.FromResult<IFile>(ret);
		}

        void InternalCreateFile(string path)
        {
            using (var stream = new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, Root))
            {
            }
        }

		public Task<IFile> GetFileAsync(string name)
		{
            EnsureExists();

			string path = System.IO.Path.Combine(Path, name);
			if (!Root.FileExists(path))
			{
                throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + path);
			}
			var ret = new IsoStoreFile(name, this);
			return TaskEx.FromResult<IFile>(ret);
		}

		public Task<IList<IFile>> GetFilesAsync()
		{
            EnsureExists();

			string[] fileNames = Root.GetFileNames(System.IO.Path.Combine(Path, "*.*"));
			IList<IFile> ret = fileNames.Select(fn => new IsoStoreFile(fn, this)).Cast<IFile>().ToList().AsReadOnly();
			return TaskEx.FromResult(ret);
		}

		public async Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option)
		{
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
                    await folderToDelete.DeleteAsync();
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

		public Task<IFolder> GetFolderAsync(string name)
		{
            EnsureExists();

			string path = System.IO.Path.Combine(Path, name);
			if (!Root.DirectoryExists(path))
			{
				throw new PCLStorage.Exceptions.DirectoryNotFoundException("Directory does not exist: " + path);
			}
			var ret = new IsoStoreFolder(name, this);
			return TaskEx.FromResult<IFolder>(ret);
		}

		public Task<IList<IFolder>> GetFoldersAsync()
		{
            EnsureExists();

			string[] folderNames = Root.GetDirectoryNames(System.IO.Path.Combine(Path, "*"));
			IList<IFolder> ret = folderNames.Select(fn => new IsoStoreFolder(fn, this)).Cast<IFolder>().ToList().AsReadOnly();
			return TaskEx.FromResult(ret);
		}

		public async Task DeleteAsync()
		{
            EnsureExists();

            if (string.IsNullOrEmpty(Path))
            {
                throw new IOException("Cannot delete root Isolated Storage folder.");
            }

            foreach (var subfolder in await GetFoldersAsync())
            {
                await subfolder.DeleteAsync();
            }

            foreach (var file in await GetFilesAsync())
            {
                await file.DeleteAsync();
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
