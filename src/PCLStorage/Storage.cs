using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCLStorage
{
    public static class Storage
    {
        public static IFolder AppLocalStorage
        {
            get
            {
                return FileSystem.Current.LocalStorage;
//#if SILVERLIGHT
//                return new IsoStoreFolder(System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication());
//#elif NETFX_CORE
//                return new WinRTFolder(Windows.Storage.ApplicationData.Current.LocalFolder);
//#elif FILE_SYSTEM
//                //  SpecialFolder.LocalApplicationData is not app-specific, so use the Windows Forms API to get the app data path
//                //var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
//                var localAppData = System.Windows.Forms.Application.LocalUserAppDataPath;
//                return new FileSystemFolder(localAppData);
//#else
//                throw Storage.NotImplementedInReferenceAssembly();
//#endif
            }
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the PCLStorage NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
