﻿using Android.App;
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
        public static async Task<IFile> GetFileFromAppBundleAsync(string assetFolderFilePath, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(assetFolderFilePath, "assetFolderFilePath");
            await AwaitExtensions.SwitchOffMainThreadAsync(cancellationToken);

            //I was not able to access files from the Assets folder like 'file://android_asset/'
            //Now I copy the file into a temp folder first
            //Hopefully somebody knows a better solution for this
            //Stream iStream = null;
            //try
            //{
            //    iStream = Application.Context.Assets.Open(path);
            //}
            //catch (Exception)
            //{
            //    System.Diagnostics.Debug.WriteLine("JK#237# GetFileFromAppBundleAsync Error file (" + path + ") must be in the assets folder marked as AndroidAsset.");
            //}
            //if (iStream == null)
            //{
            //    return null;
            //}
            var tempPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            tempPath = System.IO.Path.Combine(tempPath, "appbundlefilestempfolder");
            if (System.IO.Directory.Exists(tempPath) == false)
            {
                System.IO.Directory.CreateDirectory(tempPath);
            }
            var path = System.IO.Path.GetFileName(assetFolderFilePath); //No subfolder inside the tempfolder
            tempPath = System.IO.Path.Combine(tempPath, path);
            //Files from the app package can't change so there is no need to copy them again
            if (System.IO.File.Exists(tempPath) == false)
            {
                try
                {
                    using (var br = new BinaryReader(Application.Context.Assets.Open(assetFolderFilePath)))
                    {
                        using (var bw = new BinaryWriter(new FileStream(tempPath, FileMode.Create)))
                        {
                            byte[] buffer = new byte[2048];
                            int length = 0;
                            while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                bw.Write(buffer, 0, length);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("JK#237# GetFileFromAppBundleAsync Error file (" + path + ") must be in the assets folder marked as AndroidAsset. " + e.Message);
                }
                //var oStream = new FileOutputStream(tempPath);
                //byte[] buffer = new byte[2048];
                //int length = 2048;
                //while (iStream.Read(buffer, 0, length) > 0)
                //{
                //    oStream.Write(buffer, 0, length);
                //}
                //oStream.Flush();
                //oStream.Close();
                //iStream.Close();
            }
            if (System.IO.File.Exists(tempPath) == false)
            {
                return null;
            }
            return new FileSystemFile(tempPath);
        }

    }
}
