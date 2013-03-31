using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCLStorage
{
    public static class FileSystem
    {
        static IFileSystem _fileSystem = CreateFileSystem();

        public static IFileSystem Current
        {
            get
            {
                if (_fileSystem == null)
                {
                    throw FileSystem.NotImplementedInReferenceAssembly();
                }
                return _fileSystem;
            }
        }

        static IFileSystem CreateFileSystem()
        {
#if SILVERLIGHT
			return new IsoStoreFileSystem();
#elif NETFX_CORE
			return new WinRTFileSystem();
#elif FILE_SYSTEM
            return new DesktopFileSystem();
#else
            return null;
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the PCLStorage NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
