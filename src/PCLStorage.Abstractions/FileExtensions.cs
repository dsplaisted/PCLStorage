using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    /// <summary>
    /// Provides extension methods for the <see cref="IFile"/> class
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Reads the contents of a file as a string
        /// </summary>
        /// <param name="file">The file to read </param>
        /// <returns>The contents of the file</returns>
        public static async Task<string> ReadAllTextAsync(this IFile file)
        {
            using (var stream = await file.OpenAsync(FileAccess.Read).ConfigureAwait(false))
            {
                using (var sr = new StreamReader(stream))
                {
                    string text = await sr.ReadToEndAsync().ConfigureAwait(false);
                    return text;
                }
            }
        }

        /// <summary>
        /// Writes text to a file, overwriting any existing data
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <param name="contents">The content to write to the file</param>
        /// <returns>A task which completes when the write operation finishes</returns>
        public static async Task WriteAllTextAsync(this IFile file, string contents)
        {
            using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite).ConfigureAwait(false))
            {
                stream.SetLength(0);
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteAsync(contents).ConfigureAwait(false);
                }
            }
        }
    }
}
