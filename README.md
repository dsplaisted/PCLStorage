# PCL Storage

![PCL Storage](https://dsplaisted.blob.core.windows.net/oss/pickles_64.png)

PCL Storage provides a consistent, portable set of local file IO APIs for .NET,
Windows Phone, Windows Store, Xamarin.iOS, Xamarin.Android, and Silverlight.
This makes it easier to create cross-platform .NET libraries and apps.

Here is a sample showing how you can use PCL Storage to create a folder and
write to a text file in that folder:

```C#
public async Task PCLStorageSample()
{
    IFolder rootFolder = FileSystem.Current.LocalStorage;
    IFolder folder = await rootFolder.CreateFolderAsync("MySubFolder",
        CreationCollisionOption.OpenIfExists);
    IFile file = await folder.CreateFileAsync("answer.txt",
        CreationCollisionOption.ReplaceExisting);
    await file.WriteAllTextAsync("42");
}
```

## Installation

Install the [PCLStorage NuGet Package](http://nuget.org/packages/pclstorage).

If you reference the package from a Portable Class Library, you will also need
to reference the package from each platform-specific app. This is because the
Portable Class Library version of PCL Storage doesn't contain the actual
implementation of the storage APIs (because it differs from platform to
platform), so referencing the package from an app will ensure that the
platform-specific version of PCL Storage is included in the app and used at
runtime.

## Background information

Different .NET platforms have different APIs for accessing the file system or
an app-local persisted storage area. The full .NET Framework provides the
standard file and directory APIs (in the System.IO namespace), Silverlight and
Windows Phone provide isolated storage APIs, and WinRT provides storage APIs in
the Windows.Storage namespace.

These differing APIs make it harder to write cross-platform code. Traditionally,
you could handle this via conditional compilation. However, that means you can't
take advantage of Portable Class Libraries, and in any case may not scale well
as your code gets complex (and especially because for WinRT you need to use
async APIs).

Alternatively, you can create an abstraction for the functionality you need
across platforms, and implement the abstraction for each platform you need to
use. This approach allows you to use Portable Class Libraries, and in general
makes your code cleaner and more maintainable by isolating the platform-specific
pieces instead of having them sprinkled arbitrarily throughout your code.

Writing an abstraction layer is a bit of a barrier to entry to writing
cross-platform code, and there's no reason everyone should have to do it
separately for functionality as commonly needed as local file IO. PCL Storage
aims to provide a common abstraction that is easy to take advantage of.

## APIs

The primary APIs in PCL Storage are the IFile, IFolder, and IFileSystem
interfaces. The APIs should be mostly self-explanatory and should feel very
familiar if you have used the WinRT storage APIs.

The IFileSystem interface is the main API entry point. You can get an instance
of the implementation for the current platform with the FileSystem.Current
property.

```C#
namespace PCLStorage
{
    public static class FileSystem
    {
        public static IFileSystem Current { get; }
    }

    public interface IFileSystem
    {
        IFolder LocalStorage { get; }
        IFolder RoamingStorage { get; }

        Task<IFile> GetFileFromPathAsync(string path);
        Task<IFolder> GetFolderFromPathAsync(string path);
    }

    public enum CreationCollisionOption
    {
        GenerateUniqueName = 0,
        ReplaceExisting = 1,
        FailIfExists = 2,
        OpenIfExists = 3,
    }

    public interface IFolder
    {
        string Name { get; }
        string Path { get; }

        Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption option);
        Task<IFile> GetFileAsync(string name);
        Task<IList<IFile>> GetFilesAsync();

        Task<IFolder> CreateFolderAsync(string desiredName,
            CreationCollisionOption option);
        Task<IFolder> GetFolderAsync(string name);
        Task<IList<IFolder>> GetFoldersAsync();

        Task DeleteAsync();
    }

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

    public static class PortablePath
    {
        public static char DirectorySeparatorChar { get; }
        public static string Combine(params string[] paths);
    }
    public static class FileExtensions
    {
        public static async Task<string> ReadAllTextAsync(this IFile file)
        public static async Task WriteAllTextAsync(this IFile file, string contents);
    }
}
```
