using System.Runtime.InteropServices;

namespace Woof.SystemEx.Win32Types {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct LocalGroupName {
        [MarshalAs(UnmanagedType.LPWStr)] internal string Value;
    }

}