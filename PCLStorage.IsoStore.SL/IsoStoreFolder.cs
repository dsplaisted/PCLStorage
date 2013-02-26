using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PCLStorage
{
	public class IsoStoreFolder : IFolder
	{

		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		public string Path
		{
			get { throw new NotImplementedException(); }
		}

		public System.Threading.Tasks.Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option)
		{
			throw new NotImplementedException();
		}

		public System.Threading.Tasks.Task<IFile> GetFileAsync(string name)
		{
			throw new NotImplementedException();
		}

		public System.Threading.Tasks.Task<System.Collections.Generic.IList<IFile>> GetFilesAsync()
		{
			throw new NotImplementedException();
		}

		public System.Threading.Tasks.Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption option)
		{
			throw new NotImplementedException();
		}

		public System.Threading.Tasks.Task<IFolder> GetFolderAsync(string name)
		{
			throw new NotImplementedException();
		}

		public System.Threading.Tasks.Task<System.Collections.Generic.IList<IFolder>> GetFoldersAsync()
		{
			throw new NotImplementedException();
		}

		public System.Threading.Tasks.Task DeleteAsync()
		{
			throw new NotImplementedException();
		}

		internal Task<Stream> OpenFileAsync(string filename, FileAccess fileAccess)
		{


			return TaskEx.FromResult<Stream>(null);
		}
	}
}
