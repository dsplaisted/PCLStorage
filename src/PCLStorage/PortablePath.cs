using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PCLStorage
{
    /// <summary>
    /// Provides portable versions of APIs such as Path.Combine
    /// </summary>
    public static class PortablePath
    {
        /// <summary>
        /// The character used to separate elements in a file system path
        /// </summary>
        public static char DirectorySeparatorChar
        {
            get
            {
#if NETFX_CORE
				return '\\';
#elif PORTABLE
                throw FileSystem.NotImplementedInReferenceAssembly();
#else
                return Path.DirectorySeparatorChar;
#endif
            }
        }

        /// <summary>
        /// Combines multiple strings into a path
        /// </summary>
        /// <param name="paths">Path elements to combine</param>
        /// <returns>A combined path</returns>
        public static string Combine(params string[] paths)
        {
#if PORTABLE
            throw FileSystem.NotImplementedInReferenceAssembly();
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
