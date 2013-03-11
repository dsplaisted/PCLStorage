using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    [DebuggerDisplay("{Name = {_name}")]
    public class FileSystemFolder : IFolder
    {
        readonly string _name;
        readonly string _path;
        readonly bool _canDelete;

        private FileSystemFolder(string path, bool canDelete)
        {
            _name = System.IO.Path.GetFileName(path);
            _path = path;
            _canDelete = canDelete;
        }

        public FileSystemFolder(string path)
            : this(path, false)
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

        public Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option)
        {
            EnsureExists();

            string nameToUse = desiredName;
            string newPath = System.IO.Path.Combine(Path, nameToUse);
            if (File.Exists(newPath))
            {
                if (option == CreationCollisionOption.GenerateUniqueName)
                {
                    string desiredRoot = System.IO.Path.GetFileNameWithoutExtension(desiredName);
                    string desiredExtension = System.IO.Path.GetExtension(desiredName);
                    for (int num = 2; File.Exists(newPath); num++)
                    {
                        nameToUse = desiredRoot + " (" + num + ")" + desiredExtension;
                        newPath = System.IO.Path.Combine(Path, nameToUse);
                    }
                    InternalCreateFile(newPath);
                }
                else if (option == CreationCollisionOption.ReplaceExisting)
                {
                    File.Delete(newPath);
                    InternalCreateFile(newPath);
                }
                else if (option == CreationCollisionOption.FailIfExists)
                {
                    throw new IOException("File already exists: " + newPath);
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
                InternalCreateFile(newPath);
            }

            var ret = new FileSystemFile(newPath);
            return Task.FromResult<IFile>(ret);
        }

        void InternalCreateFile(string path)
        {
            using (var stream = File.Create(path))
            {
            }

        }

        public Task<IFile> GetFileAsync(string name)
        {
            string path = System.IO.Path.Combine(Path, name);
            if (!File.Exists(path))
            {
                throw new PCLStorage.Exceptions.FileNotFoundException("File does not exist: " + path);
            }
            var ret = new FileSystemFile(path);
            return Task.FromResult<IFile>(ret);
        }

        public Task<IList<IFile>> GetFilesAsync()
        {
            EnsureExists();
            IList<IFile> ret = Directory.GetFiles(Path).Select(f => new FileSystemFile(f)).ToList<IFile>().AsReadOnly();
            return Task.FromResult(ret);
        }

        public Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option)
        {
            EnsureExists();
            string nameToUse = desiredName;
            string newPath = System.IO.Path.Combine(Path, nameToUse);
            if (Directory.Exists(newPath))
            {
                if (option == CreationCollisionOption.GenerateUniqueName)
                {
                    for (int num = 2; Directory.Exists(newPath); num++)
                    {
                        nameToUse = desiredName + " (" + num + ")";
                        newPath = System.IO.Path.Combine(Path, nameToUse);
                    }
                    Directory.CreateDirectory(newPath);
                }
                else if (option == CreationCollisionOption.ReplaceExisting)
                {
                    Directory.Delete(newPath, true);
                    Directory.CreateDirectory(newPath);
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
                Directory.CreateDirectory(newPath);
            }

            var ret = new FileSystemFolder(newPath, true);
            return Task.FromResult<IFolder>(ret);
        }

        public Task<IFolder> GetFolderAsync(string name)
        {
            string path = System.IO.Path.Combine(Path, name);
            if (!Directory.Exists(path))
            {
                throw new PCLStorage.Exceptions.DirectoryNotFoundException("Directory does not exist: " + path);
            }
            var ret = new FileSystemFolder(path, true);
            return Task.FromResult<IFolder>(ret);
        }

        public Task<IList<IFolder>> GetFoldersAsync()
        {
            EnsureExists();
            IList<IFolder> ret = Directory.GetDirectories(Path).Select(d => new FileSystemFolder(d, true)).ToList<IFolder>().AsReadOnly();
            return Task.FromResult(ret);
        }

        public Task DeleteAsync()
        {
            if (!_canDelete)
            {
                throw new IOException("Cannot delete root storage folder.");
            }
            EnsureExists();
            Directory.Delete(Path, true);
            return Task.FromResult(true);
        }

        void EnsureExists()
        {
            if (!Directory.Exists(Path))
            {
                throw new PCLStorage.Exceptions.DirectoryNotFoundException("Directory does not exist: " + Path);
            }
        }
    }
}
