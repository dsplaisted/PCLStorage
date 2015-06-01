using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PCLStorage
{
    /// <summary>
    /// Provides extension methods for the <see cref="TargetPlatformFileSystem"/> class
    /// </summary>
    public class TargetPlatformFileSystem
    {
        /// <summary>
        /// Writes a stream to a file, overwriting any existing data
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <param name="stream">The content to write to the file</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which completes when the write operation finishes</returns>
        public static async Task<bool> WriteStreamAsync(IFile file, Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null) return false;
            if (file == null) return false;
            bool success = true;
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
            if (file != null)
            {
                try
                {
                    if (stream != null)
                    {
                        using (StorageStreamTransaction transaction = await storageFile.OpenTransactedWriteAsync())
                        {
                            using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                            {
                                dataWriter.WriteBytes(await ReadFully(stream, cancellationToken));
                                transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                                await transaction.CommitAsync();
                            }
                        }
                    }
                    else
                    {
                        //JK.JKTools.JKLog("JK#66# The text box is empty, please write something and then click 'Write' again.");
                        success = false;
                    }
                }
                catch (FileNotFoundException)
                {
                    //JK.JKTools.JKLog("JK#67# NotifyUserFileNotExist()");
                    success = false;
                }
            }
            else
            {
                //JK.JKTools.JKLog("JK#68# NotifyUserFileNotExist()");
                success = false;
            }
            return success;
        }

        //Helper for WriteStreamAsync
        private static async Task<byte[]> ReadFully(Stream input, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await ms.WriteAsync(buffer, 0, read);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Extract a zip file.
        /// </summary>
        /// <param name="desinationFolder">The destination folder for zip file extraction</param>
        /// <param name="collisionOption">How to deal with collisions with existing files.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task with a List of strings containing the names of extracted files from the zip archive.</returns>
        public static async Task<List<string>> ExtractZip(IFile zipFile, IFolder desinationFolder, NameCollisionOption collisionOption = NameCollisionOption.ReplaceExisting, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

    }
}
