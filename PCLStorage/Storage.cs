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
#if SILVERLIGHT
				return new IsoStoreFolder(System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication());
#elif NETFX_CORE
				return new WinRTFolder(Windows.Storage.ApplicationData.Current.LocalFolder);
#else
                throw Storage.NotImplementedInReferenceAssembly();
#endif
			}
		}

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException();
        }
	}
}
