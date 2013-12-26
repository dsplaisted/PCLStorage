using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    /// <summary>
    /// Implementation of <see cref="IFileSystem"/> over classic .NET file I/O APIs
    /// </summary>
    public class DesktopFileSystem : IFileSystem
    {
        /// <summary>
        /// A folder representing storage which is local to the current device
        /// </summary>
        public IFolder LocalStorage
        {
            get
            {
                //  SpecialFolder.LocalApplicationData is not app-specific, so use the Windows Forms API to get the app data path
                //var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#if XAMARIN
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#else
                var localAppData = System.Windows.Forms.Application.LocalUserAppDataPath;
#endif
                return new FileSystemFolder(localAppData);
            }
        }

        /// <summary>
        /// A folder representing storage which may be synced with other devices for the same user
        /// </summary>
        public IFolder RoamingStorage
        {
            get
            {
#if XAMARIN
                return null;
#else
                //  SpecialFolder.ApplicationData is not app-specific, so use the Windows Forms API to get the app data path
                //var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var roamingAppData = System.Windows.Forms.Application.UserAppDataPath;
                return new FileSystemFolder(roamingAppData);
#endif
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
            if (File.Exists(path))
            {
                ret = new FileSystemFile(path);
            }
            return Task.FromResult(ret);
        }

        /// <summary>
        /// Gets a folder, given its path.  Returns null if the folder does not exist.
        /// </summary>
        /// <param name="path">The path to a folder, as returned from the <see cref="IFolder.Path"/> property.</param>
        /// <returns>A folder for the specified path, or null if it does not exist.</returns>
        public Task<IFolder> GetFolderFromPathAsync(string path)
        {
            IFolder ret = null;
            if (Directory.Exists(path))
            {
                ret = new FileSystemFolder(path, true);
            }

            return Task.FromResult(ret);
        }
    }
}
