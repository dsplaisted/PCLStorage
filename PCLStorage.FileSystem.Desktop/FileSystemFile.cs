using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    public class FileSystemFile : IFile
    {
        readonly string _name;
        readonly string _path;

        public FileSystemFile(string path)
        {
            _name = System.IO.Path.GetFileName(path);
            _path = path;
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
            Stream ret;
            if (fileAccess == FileAccess.Read)
            {
                ret = File.OpenRead(Path);
            }
            else if (fileAccess == FileAccess.ReadAndWrite)
            {
                ret = File.Open(Path, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
            }
            else
            {
                throw new ArgumentException("Unrecognized FileAccess value: " + fileAccess);
            }
            return Task.FromResult(ret);
        }

        public Task DeleteAsync()
        {
            File.Delete(Path);

            return Task.FromResult(true);
        }
    }
}
