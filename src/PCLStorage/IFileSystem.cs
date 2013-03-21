using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    public interface IFileSystem
    {
        IFolder LocalStorage { get; }
        IFolder RoamingStorage { get; }

        Task<IFile> GetFileFromPathAsync(string path);
        Task<IFolder> GetFolderFromPathAsync(string path);
    }
}
