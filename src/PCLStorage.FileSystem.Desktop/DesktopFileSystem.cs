using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    public class DesktopFileSystem : IFileSystem
    {
        public IFolder LocalStorage
        {
            get
            {
                //  SpecialFolder.LocalApplicationData is not app-specific, so use the Windows Forms API to get the app data path
                //var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var localAppData = System.Windows.Forms.Application.LocalUserAppDataPath;
                return new FileSystemFolder(localAppData);
            }
        }

        public IFolder RoamingStorage
        {
            get
            {
                //  SpecialFolder.ApplicationData is not app-specific, so use the Windows Forms API to get the app data path
                //var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var roamingAppData = System.Windows.Forms.Application.UserAppDataPath;
                return new FileSystemFolder(roamingAppData);
            }
        }

        public Task<IFile> GetFileFromPathAsync(string path)
        {
            IFile ret = null;
            if (File.Exists(path))
            {
                ret = new FileSystemFile(path);
            }
            return Task.FromResult(ret);
        }

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
