using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PCLStorage
{
    [DebuggerDisplay("Name = {Name}")]
	public class WinRTFile : IFile
	{
		StorageFile _wrappedFile;

		public WinRTFile(StorageFile wrappedFile)
		{
			_wrappedFile = wrappedFile;
		}

		public string Name
		{
			get { return _wrappedFile.Name; }
		}

		public string Path
		{
			get { return _wrappedFile.Path; }
		}

		public async Task<Stream> OpenAsync(FileAccess fileAccess)
		{
			FileAccessMode fileAccessMode;
			if (fileAccess == FileAccess.Read)
			{
				fileAccessMode = FileAccessMode.Read;
			}
			else if (fileAccess == FileAccess.ReadAndWrite)
			{
				fileAccessMode = FileAccessMode.ReadWrite;
			}
			else
			{
				throw new ArgumentException("Unrecognized FileAccess value: " + fileAccess);
			}

			var wrtStream = await _wrappedFile.OpenAsync(fileAccessMode);
			return wrtStream.AsStream();
		}

		public Task DeleteAsync()
		{
			return _wrappedFile.DeleteAsync().AsTask();
		}
	}
}
