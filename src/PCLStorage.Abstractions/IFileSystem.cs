using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    public interface IFileSystem
    {
        IFolder LocalStorage { get; }
        IFolder RoamingStorage { get; }

        /// <summary>
        /// Gets a file, given its path.  Returns null if the file does not exist.
        /// </summary>
        /// <param name="path">The path to a file, as returned from the <see cref="IFile.Path"/> property.</param>
        /// <returns>A file for the given path, or null if it does not exist.</returns>
        Task<IFile> GetFileFromPathAsync(string path);

        /// <summary>
        /// Gets a folder, given its path.  Returns null if the folder does not exist.
        /// </summary>
        /// <param name="path">The path to a folder, as returned from the <see cref="IFolder.Path"/> property.</param>
        /// <returns>A folder for the specified path, or null if it does not exist.</returns>
        Task<IFolder> GetFolderFromPathAsync(string path);
    }
}
