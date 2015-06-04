using Android.App;
using Android.Content.Res;
using Java.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCLStorage
{
    /// <summary>
    /// Implementation of <see cref="IFileSystem"/> over classic .NET file I/O APIs
    /// </summary>
    public class TargetPlatformFileSystem
    {
        /// <summary>
        /// Gets a file from the App Bundle.  Returns null if the file does not exist.
        /// </summary>
        /// <param name="path">The path to a file, as returned from the <see cref="IFile.Path"/> property.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A file for the given path, or null if it does not exist.</returns>
        public static async Task<IFile> GetFileFromAppBundleAsync(string path, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(path, "path");
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);

            //I was not able to access files from the Assets folder like 'file://android_asset/'
            //Now I copy the file into a temp folder first
            //Hopefully somebody got a better solution for this

            Stream iStream = Application.Context.Assets.Open(path);
            var tempPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            tempPath = System.IO.Path.Combine(tempPath, "appbundlefilestempfolder");
            if (System.IO.Directory.Exists(tempPath) == false)
            {
                System.IO.Directory.CreateDirectory(tempPath);
            }
            tempPath = System.IO.Path.Combine(tempPath, path);
            if (System.IO.Directory.Exists(tempPath) == false)
            {
                //Files from the app package can't change so there is no need to copy them again
                var oStream = new FileOutputStream(tempPath);
                byte[] buffer = new byte[2048];
                int length = 2048;
                while (iStream.Read(buffer, 0, length) > 0)
                {
                    oStream.Write(buffer, 0, length);
                }
                oStream.Flush();
                oStream.Close();
                iStream.Close();
            }

            if (System.IO.File.Exists(tempPath) == false)
            {
                return null;
            }
            return new FileSystemFile(tempPath);
        }

    }
}
