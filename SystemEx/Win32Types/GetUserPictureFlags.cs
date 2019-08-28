using System;

namespace Woof.SystemEx.Win32Types {

    /// <summary>
    /// Flags used by SHGetUserPicture and SHGetUserPictureEx shell32 calls.
    /// </summary>
    [Flags]
    internal enum GetUserPictureFlags : uint {

        /// <summary>
        /// Make path contain only directory.
        /// </summary>
        Directory = 0x1,

        /// <summary>
        /// Make path contain only default pictures directory.
        /// </summary>
        DefaultDirectory = 0x2,

        /// <summary>
        /// Creates the (default) pictures directory if it doesn't exist.
        /// </summary>
        CreatePicturesDir = 0x80000000

    }

}