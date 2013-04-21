using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PCLStorage.Exceptions
{
    /// <exclude/>
    public class FileNotFoundException
#if PORTABLE
 : IOException
#else
        : System.IO.FileNotFoundException
#endif
    {
        /// <exclude/>
        public FileNotFoundException(string message)
            : base(message)
        {

        }

        /// <exclude/>
        public FileNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }

    /// <exclude/>
    public class DirectoryNotFoundException
#if PORTABLE
        : IOException
#elif NETFX_CORE
        : System.IO.FileNotFoundException
#else
        : System.IO.DirectoryNotFoundException
#endif
    {
        /// <exclude/>
        public DirectoryNotFoundException(string message)
            : base(message)
        {

        }

        /// <exclude/>
        public DirectoryNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
