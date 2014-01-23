namespace PCLStorage
{
    /// <summary>
    /// Specifies what should happen when trying to create/rename a file or folder to a name that already exists.
    /// </summary>
    public enum NameCollisionOption
    {
        /// <summary>
        /// Automatically generate a unique name by appending a number to the name of
        /// the file or folder.
        /// </summary>
        GenerateUniqueName = 0,

        /// <summary>
        /// Replace the existing file or folder. Your app must have permission to access
        /// the location that contains the existing file or folder.  Access to a location
        /// can be granted in several ways, for example, by a capability declared in
        /// your application's manifest, or by the user through the . You can use Windows.Storage.AccessCache
        /// to manage the list of locations that are accessible to your app via the .
        /// </summary>
        ReplaceExisting = 1,

        /// <summary>
        /// Return an error if another file or folder exists with the same name and abort
        /// the operation.
        /// </summary>
        FailIfExists = 2,
    }
}
