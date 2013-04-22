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
    /// <summary>
    /// Implementation of <see cref="IFileSystem"/> over Isolated Storage APIs
    /// </summary>
    public class IsoStoreFileSystem : IFileSystem
    {
        IsolatedStorageFile Root = IsolatedStorageFile.GetUserStoreForApplication();

        /// <summary>
        /// A folder representing storage which is local to the current device
        /// </summary>
        public IFolder LocalStorage
        {
            get
            {
                return new IsoStoreFolder(Root);
            }
        }

        /// <summary>
        /// Returns null, as roaming storage isn't supported by the Isolated Storage APIs
        /// </summary>
        public IFolder RoamingStorage
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a file, given its path.  Returns null if the file does not exist.
        /// </summary>
        /// <param name="path">The path to a file, as returned from the <see cref="IFile.Path"/> property.</param>
        /// <returns>A file for the given path, or null if it does not exist.</returns>
        public Task<IFile> GetFileFromPathAsync(string path)
        {
            IFile ret = null;
            if (Root.FileExists(path))
            {
                ret = new IsoStoreFile(Root, path);
            }
            return TaskEx.FromResult(ret);

        }

        /// <summary>
        /// Gets a folder, given its path.  Returns null if the folder does not exist.
        /// </summary>
        /// <param name="path">The path to a folder, as returned from the <see cref="IFolder.Path"/> property.</param>
        /// <returns>A folder for the specified path, or null if it does not exist.</returns>
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
