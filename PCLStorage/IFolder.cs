using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
	public enum CreationCollisionOption
	{
		GenerateUniqueName = 0,
		ReplaceExisting = 1,
		FailIfExists = 2,
		OpenIfExists = 3,
	}

	public interface IFolder
	{
		string Name { get; }
		string Path { get; }

		Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option);
		Task<IFile> GetFileAsync(string name);
		Task<IList<IFile>> GetFilesAsync();

		Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option);
		Task<IFolder> GetFolderAsync(string name);
		Task<IList<IFolder>> GetFoldersAsync();

		Task DeleteAsync();
	}
}
