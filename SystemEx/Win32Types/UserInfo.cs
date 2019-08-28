using System;
using System.Runtime.InteropServices;

namespace Woof.SystemEx.Win32Types {

    [StructLayout(LayoutKind.Sequential)]
    internal struct UserInfo {

        [MarshalAs(UnmanagedType.LPWStr)] public string Name;
        [MarshalAs(UnmanagedType.LPWStr)] public string FullName;
        [MarshalAs(UnmanagedType.LPWStr)] public string Comment;
        public UserFlags Flags;
        public int Rid;

    }

}