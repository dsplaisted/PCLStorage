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
    /// <summary>
    /// Represents a file in the <see cref="WinRTFileSystem"/>
    /// </summary>
    [DebuggerDisplay("Name = {Name}")]
	public class WinRTFile : IFile
	{
        private readonly IStorageFile _wrappedFile;

        /// <summary>
        /// Creates a new <see cref="WinRTFile"/>
        /// </summary>
        /// <param name="wrappedFile">The WinRT <see cref="IStorageFile"/> to wrap</param>
        public WinRTFile(IStorageFile wrappedFile)
		{
			_wrappedFile = wrappedFile;
		}

        /// <summary>
        /// The name of the file
        /// </summary>
		public string Name
		{
			get { return _wrappedFile.Name; }
		}

        /// <summary>
        /// The "full path" of the file, which should uniquely identify it within a given <see cref="IFileSystem"/>
        /// </summary>
		public string Path
		{
			get { return _wrappedFile.Path; }
		}

        /// <summary>
        /// Opens the file
        /// </summary>
        /// <param name="fileAccess">Specifies whether the file should be opened in read-only or read/write mode</param>
        /// <returns>A <see cref="Stream"/> which can be used to read from or write to the file</returns>
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

        /// <summary>
        /// Deletes the file
        /// </summary>
        /// <returns>A task which will complete after the file is deleted.</returns>
		public Task DeleteAsync()
		{
			return _wrappedFile.DeleteAsync().AsTask();
		}
	}
}
