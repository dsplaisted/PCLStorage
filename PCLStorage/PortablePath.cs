using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PCLStorage
{
    public static class PortablePath
    {
        public static char DirectorySeparatorChar
        {
            get
            {
#if SILVERLIGHT
				return Path.DirectorySeparatorChar;
#elif NETFX_CORE
				return '\\';
#else
                throw Storage.NotImplementedInReferenceAssembly();
#endif
            }
        }

        public static string Combine(params string[] paths)
        {
#if PORTABLE
            throw Storage.NotImplementedInReferenceAssembly();
#else
            return Path.Combine(paths);
#endif
        }
    }
}
