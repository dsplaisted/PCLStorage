using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.IO;

namespace PCLStorage
{
	public class IsoStoreFile : IFile
	{
		readonly string _name;
		readonly IsoStoreFolder _parentFolder;
		readonly Lazy<string> _path;

		public IsoStoreFile(string name, IsoStoreFolder parentFolder)
		{
			_name = name;
			_parentFolder = parentFolder;
			_path = new Lazy<string>(() => System.IO.Path.Combine(_parentFolder.Path, _name));
		}

		public string Name
		{
			get { return _name; }
		}

		public string Path
		{
			get { return _path.Value; }
		}

		public Task<Stream> OpenAsync(FileAccess fileAccess)
		{
			throw new NotImplementedException();
		}

		public System.Threading.Tasks.Task DeleteAsync()
		{
			throw new NotImplementedException();
		}
	}
}
