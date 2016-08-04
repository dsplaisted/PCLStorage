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

        internal static FileSystemFileStats FromPath(string path)
        {
            
            var fileInfo = new FileInfo(path);

            return new FileSystemFileStats()
            {
                Name = fileInfo.Name,
                Extension = fileInfo.Extension,
                CreationTime = fileInfo.CreationTime,
                CreationTimeUTC = fileInfo.CreationTimeUtc,
                LastWriteTime = fileInfo.LastWriteTime,
                LastWriteTimeUTC = fileInfo.LastWriteTimeUtc,
                LastAccessTime = fileInfo.LastAccessTime,
                LastAccessTimeUTC = fileInfo.LastAccessTimeUtc,
                Length = fileInfo.Length
            };

        }

    }
}
