using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLStorage
{
    public static class FileExtensions
    {
        public static async Task<string> ReadAllTextAsync(this IFile file)
        {
            using (var stream = await file.OpenAsync(FileAccess.Read))
            {
                using (var sr = new StreamReader(stream))
                {
                    string text = await sr.ReadToEndAsync();
                    return text;
                }
            }
        }

        public static async Task WriteAllTextAsync(this IFile file, string contents)
        {
            using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite))
            {
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteAsync(contents);
                }
            }
        }
    }
}
