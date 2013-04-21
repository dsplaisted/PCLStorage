using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    /// <summary>
    /// Specifies what should happen when trying to create a file or folder that already exists.
    /// </summary>
    public enum CreationCollisionOption
    {
        /// <summary>
        /// Creates a new file with a unique name of the form "name (2).txt"
        /// </summary>
        GenerateUniqueName = 0,
        /// <summary>
        /// Replaces any existing file with a new (empty) one
        /// </summary>
        ReplaceExisting = 1,
        /// <summary>
        /// Throws an exception if the file exists
        /// </summary>
        FailIfExists = 2,
        /// <summary>
        /// Opens the existing file, if any
        /// </summary>
        OpenIfExists = 3,
    }

    /// <summary>
    /// Represents a file system folder
    /// </summary>
    public interface IFolder
    {
        /// <summary>
        /// The name of the folder
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The "full path" of the folder, which should uniquely identify it within a given <see cref="IFileSystem"/>
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Creates a file in this folder
        /// </summary>
        /// <param name="desiredName">The name of the file to create</param>
        /// <param name="option">Specifies how to behave if the specified file already exists</param>
        /// <returns>The newly created file</returns>
        Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option);

        /// <summary>
        /// Gets a file in this folder
        /// </summary>
        /// <param name="name">The name of the file to get</param>
        /// <returns>The requested file, or null if it does not exist</returns>
        Task<IFile> GetFileAsync(string name);

        /// <summary>
        /// Gets a list of the files in this folder
        /// </summary>
        /// <returns>A list of the files in the folder</returns>
        Task<IList<IFile>> GetFilesAsync();


        /// <summary>
        /// Creates a subfolder in this folder
        /// </summary>
        /// <param name="desiredName">The name of the folder to create</param>
        /// <param name="option">Specifies how to behave if the specified folder already exists</param>
        /// <returns>The newly created folder</returns>
        Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option);

        /// <summary>
        /// Gets a subfolder in this folder
        /// </summary>
        /// <param name="name">The name of the folder to get</param>
        /// <returns>The requested folder, or null if it does not exist</returns>
        Task<IFolder> GetFolderAsync(string name);

        /// <summary>
        /// Gets a list of subfolders in this folder
        /// </summary>
        /// <returns>A list of subfolders in the folder</returns>
        Task<IList<IFolder>> GetFoldersAsync();

        /// <summary>
        /// Deletes this folder and all of its contents
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync();
    }
}
