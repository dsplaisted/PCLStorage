using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCLStorage
{
    /// <summary>
    /// FileStats
    /// </summary>
    public interface IFileStats
    {

        /// <summary>
        /// Name
        /// </summary>
        /// <value>The name</value>
        string Name { get; }

        /// <summary>
        /// Extension
        /// </summary>
        /// <value>The extension</value>
        string Extension { get; }

        /// <summary>
        /// Creation time.
        /// </summary>
        /// <value>The creation time.</value>
        DateTime CreationTime { get; }

        /// <summary>
        /// Creation time UTC.
        /// </summary>
        /// <value>The creation time UTC.</value>
        DateTime CreationTimeUTC { get; }

        /// <summary>
        /// Last access time.
        /// </summary>
        /// <value>The last access time.</value>
        DateTime LastAccessTime { get;}

        /// <summary>
        /// Last access time UTC.
        /// </summary>
        /// <value>The last access time UTC.</value>
        DateTime LastAccessTimeUTC { get; }

        /// <summary>
        /// Last write time.
        /// </summary>
        /// <value>The last write time.</value>
        DateTime LastWriteTime { get; }

        /// <summary>
        /// Last write time UTC.
        /// </summary>
        /// <value>The last write time UTC.</value>
        DateTime LastWriteTimeUTC { get; }



        /// <summary>
        /// Length of the data
        /// </summary>
        /// <value>The length.</value>
        long Length { get; }

    }
}
