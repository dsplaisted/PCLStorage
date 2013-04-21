using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    /// <summary>
    /// Specifies whether a file should be opened for write access or not
    /// </summary>
    public enum FileAccess
    {
        /// <summary>
        /// Specifies that a file should be opened for read-only access
        /// </summary>
        Read,
        /// <summary>
        /// Specifies that a file should be opened for read/write access
        /// </summary>
        ReadAndWrite
    }

    /// <summary>
    /// Represents a file
    /// </summary>
    public interface IFile
    {
       /// <summary>
       /// The name of the file
       /// </summary>
        string Name { get; }
        /// <summary>
        /// The "full path" of the file, which should uniquely identify it within a given <see cref="IFileSystem"/>
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Opens the file
        /// </summary>
        /// <param name="fileAccess">Specifies whether the file should be opened in read-only or read/write mode</param>
        /// <returns>A <see cref="Stream"/> which can be used to read from or write to the file</returns>
        Task<Stream> OpenAsync(FileAccess fileAccess);

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <returns>A task which will complete after the file is deleted.</returns>
        Task DeleteAsync();
    }
}
