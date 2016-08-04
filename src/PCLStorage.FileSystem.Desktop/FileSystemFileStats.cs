using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    class FileSystemFileStats : IFileStats
    {
        #region IFileStats Member

        public string Name { get; internal set; }
        public string Extension { get; internal set; }
        public DateTime CreationTime { get; internal set; }
        public DateTime CreationTimeUTC { get; internal set; }

        public DateTime LastAccessTime { get; internal set; }
        public DateTime LastAccessTimeUTC { get; internal set; }

        public DateTime LastWriteTime { get; internal set; }
        public DateTime LastWriteTimeUTC { get; internal set; }

        public long Length { get; internal set; }

        #endregion

#if !WINDOWS_PHONE

        internal static FileSystemFileStats FromPath(string path)
        {



            var fileInfo = new FileInfo(path);

            var fileStats = new FileSystemFileStats()
            {
                Name = fileInfo.Name,
                Extension = fileInfo.Extension,
                CreationTime = fileInfo.CreationTime,
                LastWriteTime = fileInfo.LastWriteTime,
                LastAccessTime = fileInfo.LastAccessTime,
                Length = fileInfo.Length
            };

#if !SILVERLIGHT
            fileStats.CreationTimeUTC = fileInfo.CreationTimeUtc;
            fileStats.LastWriteTimeUTC = fileInfo.LastWriteTimeUtc;
            fileStats.LastAccessTimeUTC = fileInfo.LastAccessTimeUtc;
#endif

            return fileStats;



        }
#endif

    }

}
