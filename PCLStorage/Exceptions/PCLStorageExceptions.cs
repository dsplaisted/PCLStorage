using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PCLStorage.Exceptions
{
    public class FileNotFoundException
#if PORTABLE
 : IOException
#else
        : System.IO.FileNotFoundException
#endif
    {
        public FileNotFoundException(string message)
            : base(message)
        {

        }

        public FileNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }

    public class DirectoryNotFoundException
#if PORTABLE || NETFX_CORE
 : IOException
#else
        : System.IO.DirectoryNotFoundException
#endif
    {
        public DirectoryNotFoundException(string message)
            : base(message)
        {

        }

        public DirectoryNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
