using System;
using System.Runtime.InteropServices;

namespace Woof.SystemEx.Win32Types {

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct LocalGroupMember {
        public IntPtr PSid;
        public SidNameUse SidUsage;
        [MarshalAs(UnmanagedType.LPWStr)] public string DomainAndName;
    }

}