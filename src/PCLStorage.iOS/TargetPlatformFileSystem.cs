using MonoTouch.Foundation;
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
    /// Implementation of <see cref="TargetPlatformFileSystem"/> with platform specific Foundation methods
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

            string bundlePath = NSBundle.MainBundle.ResourcePath;
            bundlePath = Path.Combine(bundlePath, path);
            FileSystemFile f = null;
            if (File.Exists(bundlePath))
            {
                f = new FileSystemFile(bundlePath);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("JK#237b# GetFileFromAppBundleAsync Error file (" + path + ") must be in the Resources folder marked as BundleResource.");
            }
            return f;
        }

    }
}
