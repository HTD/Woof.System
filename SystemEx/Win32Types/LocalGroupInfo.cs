using System.Runtime.InteropServices;

namespace Woof.SystemEx.Win32Types {

    [StructLayout(LayoutKind.Sequential)]
    internal struct LocalGroupInfo {
        [MarshalAs(UnmanagedType.LPWStr)] public string Name;
        [MarshalAs(UnmanagedType.LPWStr)] public string Comment;
    }

}