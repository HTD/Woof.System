using MbnApi; // add to references from: C:\Program Files (x86)\Windows Kits\10\Lib\{Build}\um\x86, requires Windows 10 SDK.
using Microsoft.VisualBasic.Devices; // add to references.
using Microsoft.Win32;
using System;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess; // add to references.
using System.Text;

namespace Woof.SystemEx { // depends on Woof.SysInternals.WMI, Woof.Identification.DGuid, MbnApi.

    /// <summary>
    /// Provide system information.
    /// </summary>
    public static class SysInfo {

        /// <summary>
        /// Gets all WMI data on first CPU in the system.
        /// </summary>
        public static dynamic Cpu => WMI.Query("SELECT * FROM Win32_Processor").First();

        /// <summary>
        /// Gets first CPU ProcessorId string.
        /// </summary>
        public static string CpuId => WMI.Query("SELECT ProcessorId FROM Win32_Processor").First().ProcessorId;

        /// <summary>
        /// Gets the number of first CPU cores.
        /// </summary>
        public static int CpuCores => _CpuCores ?? (_CpuCores = (int)WMI.Query("SELECT NumberOfCores FROM Win32_Processor").First().NumberOfCores).Value;

        /// <summary>
        /// Gets the number of physical CPUs in the system.
        /// </summary>
        public static int CpuCount => _CpuCount ?? (_CpuCount = WMI.Query("SELECT NumberOfCores FROM Win32_Processor").Length).Value;

        /// <summary>
        /// Gets the CPU clock speed returned as MaxClockSpeed by Win32_Processor WMI class. Can be different from actual frequency.
        /// </summary>
        public static int CpuSpeed => _CpuSpeed ?? (_CpuSpeed = (int)WMI.Query("SELECT MaxClockSpeed FROM Win32_Processor").First().MaxClockSpeed).Value;

        /// <summary>
        /// Gets the full user name (with domain) of the user that is currently logged on.
        /// </summary>
        /// <remarks>
        /// Returns null when no user is logged on.
        /// </remarks>
        public static string LogonUserFullName => WMI.Query("SELECT UserName FROM Win32_ComputerSystem").FirstOrDefault()?.UserName;

        /// <summary>
        /// Gets the domain name of the user that is currently logged on.
        /// </summary>
        /// <remarks>
        /// Returns null when no user is logged on.
        /// </remarks>
        public static string LogonUserDomain => LogonUserFullName.Split('\\').FirstOrDefault();

        /// <summary>
        /// Gets the (short) name of the user that is currently logged on.
        /// </summary>
        /// <remarks>
        /// Returns null when no user is logged on.
        /// </remarks>
        public static string LogonUserName => LogonUserFullName.Split('\\').Skip(1).FirstOrDefault();

        /// <summary>
        /// Gets a value indicating that a user currently logged on is in Administrators group.
        /// </summary>
        public static bool IsLogonUserAdmin => Users.FirstOrDefault(i => i.SID == LogonUserSid.Value).IsAdmin;

        /// <summary>
        /// Gets the security identifier of the user that is currently logged on.
        /// </summary>
        public static SecurityIdentifier LogonUserSid => new SecurityIdentifier(Users.FirstOrDefault(i => i.Name == LogonUserName).SID);

        /// <summary>
        /// Gets the current (normal, system, elevated) user SID.
        /// </summary>
        public static SecurityIdentifier UserSid => WindowsIdentity.GetCurrent().User;

        /// <summary>
        /// Gets the current (normal, system, elevated) user Name.
        /// </summary>
        public static string UserName => WindowsIdentity.GetCurrent().Name.Split('\\').Skip(1).FirstOrDefault();

        /// <summary>
        /// Gets unique device identifier based on mobile broadband network interface IMEI, system disk serial number or concatenation of physical networ adapters MAC addresses.
        /// </summary>
        public static Guid DeviceId => _DeviceId ?? (_DeviceId = new DGuid(IMEI ?? SystemDiskSerialNumber ?? String.Join(" ", PhysicalMacs)));

        /// <summary>
        /// Gets IMEI (International Mobile Equipment Identity) for mobile devices, null for dekstops.
        /// </summary>
        public static string IMEI => _IMEI ?? (_IMEI = GetIMEI());

        /// <summary>
        /// Gets local AppData directory for current user, WORKS FROM SYSTEM ACCOUNT!
        /// </summary>
        public static string LocalAppDataDir => Path.Combine(ProfilesDirectory, LogonUserName, "AppData", "Local");

        /// <summary>
        /// Gets total installed RAM amount in GB.
        /// </summary>
        public static double MemoryTotal {
            get {
                if (_MemoryTotal > 0) return _MemoryTotal;
                NativeMethods.GetPhysicallyInstalledSystemMemory(out var p);
                return _MemoryTotal = (uint)p / (double)0x10_0000L;
            }
        }

        /// <summary>
        /// Gets physical network interfaces MAC addresses (hex values delimited by ':').
        /// </summary>
        public static string[] PhysicalMacs => WMI.Query("SELECT MACAddress FROM Win32_NetworkAdapterConfiguration WHERE MACAddress IS NOT NULL AND NOT Description LIKE \"%Virtual%\"").Select(i => (string)i.MACAddress).ToArray();

        /// <summary>
        /// Gets user profiles directory from Windows registry.
        /// </summary>
        public static string ProfilesDirectory => _ProfilesDirectory ?? (_ProfilesDirectory = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList", "ProfilesDirectory", ""));

        /// <summary>
        /// Gets system disk serial number, should be unique for unique devices.
        /// </summary>
        public static string SystemDiskSerialNumber =>
            _SystemDiskSerialNumber ?? (_SystemDiskSerialNumber = WMI.Query("SELECT SerialNumber FROM Win32_PhysicalMedia").First().SerialNumber);

        /// <summary>
        /// Gets free physical RAM in GB available to operating system.
        /// </summary>
        public static double SystemMemoryFree => ComputerInfo.AvailablePhysicalMemory / (double)0x4000_0000L;

        /// <summary>
        /// Gets total physical RAM in GB available to operating system.
        /// </summary>
        public static double SystemMemoryTotal {
            get {
                if (_SystemMemoryTotal > 0) return _SystemMemoryTotal;
                return _SystemMemoryTotal = ComputerInfo.TotalPhysicalMemory / (double)0x4000_0000L;
            }
        }

        /// <summary>
        /// Gets all user accounts enabled in the system.
        /// </summary>
        public static dynamic[] Users => WMI.Query("SELECT * FROM Win32_UserAccount WHERE Disabled = 0").Select<dynamic, dynamic>(d => {
            using (var machineContext = new PrincipalContext(ContextType.Machine))
            using (Principal principal = Principal.FindByIdentity(machineContext, d.SID))
                d.IsAdmin = principal.GetGroups().Any(i => i.Sid.IsWellKnown(System.Security.Principal.WellKnownSidType.BuiltinAdministratorsSid));
            return d;
        }).ToArray();

        /// <summary>
        /// Gets Windows ProductID as displayed in System Panel. 
        /// </summary>
        public static string WindowsProductId {
            get {
                if (_WindowsProductId != null) return _WindowsProductId;
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = hklm.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion")) {
                    var value = key.GetValue("ProductId");
                    return value != null ? _WindowsProductId = value.ToString() : NotAvailable;
                }
            }
        }

        /// <summary>
        /// Gets Windows ProductID in binary form.
        /// </summary>
        private static byte[] WindowsDigitalProductId {
            get {
                if (_WindowsDigitalProductId != null) return _WindowsDigitalProductId;
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = hklm.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion")) {
                    var value = key.GetValue("DigitalProductId");
                    return value != null ? _WindowsDigitalProductId = (byte[])value : null;
                }
            }
        }

        /// <summary>
        /// Gets Windows Product Key if available.
        /// </summary>
        public static string WindowsProductKey {
            get {
                if (_WindowsProductKey != null) return _WindowsProductKey;
                var digitalProductId = WindowsDigitalProductId;
                if (digitalProductId == null) return NotAvailable;
                var osVersion = Environment.OSVersion.Version;
                var currentMethod = osVersion.Major > 6 || (osVersion.Major == 6 && osVersion.Minor >= 2);
                return _WindowsProductKey = (currentMethod ? DecodeProductKey(digitalProductId) : DecodeLegacyProductKey(digitalProductId));
            }
        }

        /// <summary>
        /// Gets the <see cref="SecurityIdentifier"/> for the user name in the system.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>SID or null if the user name does not exist in the system.</returns>
        public static SecurityIdentifier GetSecurityIdentifier(string userName) {
            var sddlForm = (string)Users.FirstOrDefault(u => ((string)u.Name).Equals(userName, StringComparison.OrdinalIgnoreCase))?.SID;
            return sddlForm == null ? null : new SecurityIdentifier(sddlForm);
        }


        /// <summary>
        /// Gets Microsoft Account identifier (login) for specified SID from the Registry.
        /// </summary>
        /// <param name="sid">Account SID.</param>
        /// <returns>Microsoft Account Identifier or null if SID is not associated with a Microsoft Account.</returns>
        public static string GetMicrosoftAccount(SecurityIdentifier sid) {
            if (sid == null) return null;
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32)) {
                var path = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\Credential Providers\\{{D6886603-9D2F-4EB2-B667-1971041FA96B}}\\{sid.Value}\\UserNames";
                using (var subKey = hklm.OpenSubKey(path)) {
                    if (subKey == null) return null;
                    return subKey.GetSubKeyNames().FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Gets Microsoft Account identifier (login) for specified user name from the Registry.
        /// </summary>
        /// <param name="userName">User name or null for the current user.</param>
        /// <returns>Microsoft Account Identifier or null if user name is not associated with a Microsoft Account.</returns>
        public static string GetMicrosoftAccount(string userName) => GetMicrosoftAccount(GetSecurityIdentifier(userName ?? Environment.UserName));

        /// <summary>
        /// Generates and retrieves user tile path for the specified user name.
        /// </summary>
        /// <param name="userName">User name or null for the current user.</param>
        /// <returns>Path to the small 126x126 user account picture.</returns>
        public static string GetUserPicturePath(string userName = null) {
            if (userName == null) userName = Environment.UserName;
            var msAccountName = GetMicrosoftAccount(userName);
            var pathBuffer = new StringBuilder(1024);
            NativeMethods.SHGetUserPicturePath(msAccountName ?? userName, NativeMethods.SHGetUserPictureFlags.CreatePicturesDir, pathBuffer, pathBuffer.Capacity);
            return pathBuffer.ToString();
        }

        /// <summary>
        /// Generates and retrieves user tile path for the specified user name.
        /// </summary>
        /// <param name="userName">User name or null for the current user.</param>
        /// <param name="srcPath">Profile picture source path if available.</param>
        /// <returns>Path to the small 126x126 user account picture.</returns>
        public static string GetUserPicturePath(string userName, out string srcPath) {
            var msAccountName = GetMicrosoftAccount(userName);
            var pathBuffer = new StringBuilder(1024);
            var srcBuffer = new StringBuilder(1024);
            NativeMethods.SHGetUserPicturePathEx(msAccountName ?? userName, NativeMethods.SHGetUserPictureFlags.CreatePicturesDir, null, pathBuffer, pathBuffer.Capacity, srcBuffer, srcBuffer.Capacity);
            srcPath = srcBuffer.ToString();
            if (srcPath.StartsWith("\\\\")) { // converts machine-name format to standard windows path
                srcPath = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2) + srcPath.Substring(srcPath.IndexOf('\\', 2));   
            }
            return pathBuffer.ToString();
        }

        #region Cache

        // Cache is here for values which should not change during application session, and fetching the data from WMI or other queries is more expensive than some RAM.

        const string NotAvailable = "N/A";
        static string _IMEI;
        static string _SystemDiskSerialNumber;
        static ComputerInfo _ComputerInfo;
        static int? _CpuCount;
        static int? _CpuCores;
        static int? _CpuSpeed;
        static DGuid _DeviceId;
        static double _MemoryTotal;
        static string _ProfilesDirectory;
        static double _SystemMemoryTotal;
        static byte[] _WindowsDigitalProductId;
        static string _WindowsProductId;
        static string _WindowsProductKey;

        #endregion

        #region Toolkit

        /// <summary>
        /// Gets cached <see cref="Microsoft.VisualBasic.Devices.ComputerInfo"/> instance.
        /// </summary>
        private static ComputerInfo ComputerInfo => _ComputerInfo ?? (_ComputerInfo = new ComputerInfo());

        /// <summary>
        /// Returns IMEI of main mobile broadband network interface.
        /// </summary>
        /// <returns>IMEI or null if mobile broadband network interface is not available.</returns>
        private static string GetIMEI() {
            var mbnEnabled = false;
            using (var sc = new ServiceController("wwansvc")) mbnEnabled = sc.Status == ServiceControllerStatus.Running;
            if (!mbnEnabled) return null; // prevents throwing an exception if Mobile Broadband Service (wwansvc) is not running.
            var mbnInterfaceManager = new MbnInterfaceManager() as IMbnInterfaceManager;
            try { // ... but let's be sure...
                IMbnInterface[] mbnInterfaces = (IMbnInterface[])mbnInterfaceManager.GetInterfaces();
                return (mbnInterfaces != null && mbnInterfaces.Length > 0) ? mbnInterfaces[0].GetInterfaceCapability().deviceID : null;
            }
            catch (COMException) { return null; } // no surprises.
        }

        /// <summary>
        /// Decodes Windows 8 and 10 product key.
        /// </summary>
        /// <param name="digitalProductId">Windows ProductID as <see cref="byte"/>[] array.</param>
        /// <returns>Product key.</returns>
        private static string DecodeProductKey(byte[] digitalProductId) {
            var key = String.Empty;
            const int keyOffset = 52;
            var isWin8 = (byte)((digitalProductId[66] / 6) & 1);
            digitalProductId[66] = (byte)((digitalProductId[66] & 0xf7) | (isWin8 & 2) * 4);
            int last = 0;
            for (var i = 24; i >= 0; i--) {
                var current = 0;
                for (var j = 14; j >= 0; j--) {
                    current *= 256;
                    current = digitalProductId[j + keyOffset] + current;
                    digitalProductId[j + keyOffset] = (byte)(current / 24);
                    current %= 24;
                    last = current;
                }
                key = ProductKeyMap[current] + key;
            }
            var keypart1 = key.Substring(1, last);
            var keypart2 = key.Substring(last + 1, key.Length - (last + 1));
            key = keypart1 + "N" + keypart2;
            for (var i = 5; i < key.Length; i += 6) key = key.Insert(i, "-");
            return key;
        }

        /// <summary>
        /// Decodes legacy Windows product key.
        /// </summary>
        /// <param name="digitalProductId">Windows ProductID as <see cref="byte"/>[] array.</param>
        /// <returns>Product key.</returns>
        private static string DecodeLegacyProductKey(byte[] digitalProductId) {
            string key = String.Empty;
            for (int i = 0; i < 25; i++) {
                int mapIndex = 0;
                for (int a = 0; a < 15; a++) {
                    mapIndex <<= 8;
                    mapIndex += digitalProductId[66 - a];
                    digitalProductId[66 - a] = (byte)(mapIndex / 24 & 0xff);
                    mapIndex %= 24;
                }
                key = ProductKeyMap[mapIndex] + key;
                if (i % 5 == 4 && i < 24) key = "-" + key;
            }
            return key;
        }

        /// <summary>
        /// Character map used by Windows product keys.
        /// </summary>
        private const string ProductKeyMap = "BCDFGHJKMPQRTVWXY2346789";

        /// <summary>
        /// Win32 calls.
        /// </summary>
        static class NativeMethods {

            #region DLL imports

            /// <summary>
            /// Retrieves the amount of RAM that is physically installed on the computer.
            /// </summary>
            /// <param name="totalMemoryInKilobytes">A pointer to a variable that receives the amount of physically installed RAM, in kilobytes.</param>
            /// <returns>If the function succeeds, it returns TRUE and sets the TotalMemoryInKilobytes parameter to a nonzero value.</returns>
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool GetPhysicallyInstalledSystemMemory(out IntPtr totalMemoryInKilobytes);

            /// <summary>
            /// Copies the users account picture to a temporary directory and returns the path or returns various paths relating to user pictures.
            /// </summary>
            /// <param name="name">The name of a user account on this computer, or desired file name of the current users picture. Can be NULL to indicate the current users' name. Must be Microsoft Account Name for Microsoft Accounts.</param>
            /// <param name="flags">Options, see <see cref="SHGetUserPictureFlags"/>.</param>
            /// <param name="path">A pointer to a buffer that receives the path of the copied file. Cannot be NULL.</param>
            /// <param name="pathLength">Length of the buffer in chars.</param>
            [DllImport("shell32.dll", EntryPoint = "#261", CharSet = CharSet.Unicode, PreserveSig = false)]
            internal static extern void SHGetUserPicturePath(string name, SHGetUserPictureFlags flags, StringBuilder path, int pathLength);

            /// <summary>
            /// Copies the users account picture to a temporary directory and returns the path or returns various paths relating to user pictures.
            /// </summary>
            /// <param name="name">The name of a user account on this computer, or desired file name of the current users picture. Can be NULL to indicate the current users' name. Must be Microsoft Account Name for Microsoft Accounts.</param>
            /// <param name="flags">Options, see <see cref="SHGetUserPictureFlags"/>.</param>
            /// <param name="desiredSrcExt">Desired filetype of the source picture. Defaults to .bmp if NULL is given.</param>
            /// <param name="path">A pointer to a buffer that receives the path of the copied file. Cannot be NULL.</param>
            /// <param name="pathLength">Length of the buffer in chars.</param>
            /// <param name="srcPath">Buffer to which the original path of the users picture is copied.</param>
            /// <param name="srcLength">Length of the source path buffer in chars.</param>
            [DllImport("shell32.dll", EntryPoint = "#810", CharSet = CharSet.Unicode, PreserveSig = false)]
            internal static extern void SHGetUserPicturePathEx(string name, SHGetUserPictureFlags flags, string desiredSrcExt, StringBuilder path, int pathLength, StringBuilder srcPath, int srcLength);

            #endregion

            /// <summary>
            /// Flags used by SHGetUserPicture and SHGetUserPictureEx shell32 calls.
            /// </summary>
            [Flags]
            internal enum SHGetUserPictureFlags : uint {
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

    }

    #endregion

}