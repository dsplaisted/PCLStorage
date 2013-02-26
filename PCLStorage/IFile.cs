using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
	public enum FileAccess
	{
		Read,
		ReadAndWrite
	}

	public interface IFile
	{
		string Name { get; }
		string Path { get; }

		Task<Stream> OpenAsync(FileAccess fileAccess);
		Task DeleteAsync();
	}
}
