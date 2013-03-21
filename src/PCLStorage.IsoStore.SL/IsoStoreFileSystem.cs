using System;
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

namespace PCLStorage
{
    public class IsoStoreFileSystem : IFileSystem
    {
        IsolatedStorageFile Root = IsolatedStorageFile.GetUserStoreForApplication();

        public IFolder LocalStorage
        {
            get
            {
                return new IsoStoreFolder(Root);
            }
        }

        public IFolder RoamingStorage
        {
            get
            {
                return null;
            }
        }

        public Task<IFile> GetFileFromPathAsync(string path)
        {
            IFile ret = null;
            if (Root.FileExists(path))
            {
                ret = new IsoStoreFile(Root, path);
            }
            return TaskEx.FromResult(ret);

        }

        public Task<IFolder> GetFolderFromPathAsync(string path)
        {
            IFolder ret = null;
            if (Root.DirectoryExists(path))
            {
                ret = new IsoStoreFolder(Root, path);
            }
            return TaskEx.FromResult(ret);
        }
    }
}
