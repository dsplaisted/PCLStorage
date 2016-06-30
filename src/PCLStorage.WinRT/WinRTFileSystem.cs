﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PCLStorage
{
    /// <summary>
    /// Implementation of <see cref="IFileSystem"/> over WinRT Storage APIs
    /// </summary>
    public class WinRTFileSystem : IFileSystem
    {
        Windows.Storage.ApplicationData _applicationData;

        /// <summary>
        /// Creates a new instance of <see cref="WinRTFileSystem"/>
        /// </summary>
        public WinRTFileSystem()
        {
            _applicationData = ApplicationData.Current;
        }
        /// <summary>
        /// A folder representing storage which is local to the current device
        /// </summary>
        public IFolder LocalStorage
        {
            get
            {
                return new WinRTFolder(_applicationData.LocalFolder);
            }
        }

        /// <summary>
        /// A folder representing storage which may be synced with other devices for the same user
        /// </summary>
        public IFolder RoamingStorage
        {
            get
            {
#if WINDOWS_PHONE
                return null;
#else
                return new WinRTFolder(_applicationData.RoamingFolder);
#endif
            }
        }

        /// <summary>
        /// Gets a file, given its path.  Returns null if the file does not exist.
        /// </summary>
        /// <param name="path">The path to a file, as returned from the <see cref="IFile.Path"/> property.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A file for the given path, or null if it does not exist.</returns>
        public async Task<IFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(path, "path");

            StorageFile storageFile;
            try
            {
                storageFile = await StorageFile.GetFileFromPathAsync(path).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            return new WinRTFile(storageFile);
        }

        /// <summary>
        /// Gets a folder, given its path.  Returns null if the folder does not exist.
        /// </summary>
        /// <param name="path">The path to a folder, as returned from the <see cref="IFolder.Path"/> property.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A folder for the specified path, or null if it does not exist.</returns>
        public async Task<IFolder> GetFolderFromPathAsync(string path, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(path, "path");

            StorageFolder storageFolder;
            try
            {
                storageFolder = await StorageFolder.GetFolderFromPathAsync(path).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            return new WinRTFolder(storageFolder);
        }

        /// <summary>
        /// Gets a file from the App Bundle.  Returns null if the file does not exist.
        /// </summary>
        /// <param name="path">The path to a file, as returned from the <see cref="IFile.Path"/> property.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A file for the given path, or null if it does not exist.</returns>
        public async Task<IFile> GetFileFromAppBundleAsync(string path, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(path, "path");

            StorageFile storageFile;
            try
            {
                storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///" + path)).AsTask(cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("JK#237b# GetFileFromAppBundleAsync Error file (" + path + ") must be in the project folder marked as content.");
                return null;
            }

            return new WinRTFile(storageFile);
        }


    }
}
