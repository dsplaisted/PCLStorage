﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Stream"/> which can be used to read from or write to the file</returns>
        Task<Stream> OpenAsync(FileAccess fileAccess, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task which will complete after the file is deleted.
        /// </returns>
        Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Renames a file without changing its location.
        /// </summary>
        /// <param name="newName">The new leaf name of the file.</param>
        /// <param name="collisionOption">How to deal with collisions with existing files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task which will complete after the file is renamed.
        /// </returns>
        Task RenameAsync(string newName, NameCollisionOption collisionOption = NameCollisionOption.FailIfExists, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Moves a file.
        /// </summary>
        /// <param name="newPath">The new full path of the file.</param>
        /// <param name="collisionOption">How to deal with collisions with existing files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which will complete after the file is moved.</returns>
        Task MoveAsync(string newPath, NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets the FileStats of this file
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which will return FileStats after completion.</returns>
        Task<IFileStats> GetFileStats(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Writes creation time
        /// </summary>
        /// <param name="creationTime">The time at which the file has been created</param>
        /// <param name="utc">Set to true if you want to write utc time</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which will complete after the creation time was written.</returns>
        Task SetCreationTime(DateTime creationTime, bool utc = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Writes last access time
        /// </summary>
        /// <param name="lastAccessTime">The time at which the file has been last accessed</param>
        /// <param name="utc">Set to true if you want to write utc time</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which will complete after the last access time was written.</returns>
        Task SetLastAccessTime(DateTime lastAccessTime, bool utc = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Writes last write time
        /// </summary>
        /// <param name="lastWriteTime">The time at which the file has been last written</param>
        /// <param name="utc">Set to true if you want to write utc time</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which will complete after the last write time was written.</returns>
        Task SetLastWriteTime(DateTime lastWriteTime, bool utc = false, CancellationToken cancellationToken = default(CancellationToken));

    }
}
