using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCLStorage
{
    public interface IFileSystem
    {
        IFolder LocalStorage { get; }
        IFolder RoamingStorage { get; }

        bool FileExists(string path);
        bool FolderExists(string path);

        IFile GetFileFromPath(string path);
        IFolder GetFolderFromPath(string path);
    }
}
