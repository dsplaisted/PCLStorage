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
#elif WINDOWS_PHONE
            //  WP7 only implements Path.Combine with two arguments, so implement this in terms of that
            if (paths.Length == 0)
            {
                return string.Empty;
            }
            else if (paths.Length == 1)
            {
                return paths[0];
            }
            else
            {
                string ret = Path.Combine(paths[0], paths[1]);
                foreach (string p in paths.Skip(2))
                {
                    ret = Path.Combine(ret, p);
                }
                return ret;
            }
#else
            return Path.Combine(paths);
#endif
        }
    }
}
