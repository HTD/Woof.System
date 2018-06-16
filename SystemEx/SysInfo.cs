using System;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess; // add to references.
using MbnApi; // add to references from: C:\Program Files (x86)\Windows Kits\10\Lib\{Build}\um\x86, requires Windows 10 SDK.
using Microsoft.VisualBasic.Devices; // add to references.
using Microsoft.Win32;

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
        /// Gets the current user domain name.
        /// </summary>
        /// <remarks>
        /// Returns null when no user is logged on.
        /// </remarks>
        public static string UserDomainName {
            get {
                string fullName = WMI.Query("SELECT UserName FROM Win32_ComputerSystem").FirstOrDefault()?.UserName;
                return fullName?.Substring(0, fullName.IndexOf('\\'));
            }
        }

        /// <summary>
        /// Gets the current user (the one who is logged on) data. Works from system accounts.
        /// </summary>
        /// <remarks>
        /// Returns null when no user is logged on.
        /// </remarks>
        public static string UserName {
            get {
                string fullName = WMI.Query("SELECT UserName FROM Win32_ComputerSystem").FirstOrDefault()?.UserName;
                return fullName?.Substring(fullName.IndexOf('\\') + 1);
            }
        }

        /// <summary>
        /// Gets the current user SID. Works from system accounts.
        /// </summary>
        public static System.Security.Principal.SecurityIdentifier UserSid {
            get {
                var name = UserName;
                return new System.Security.Principal.SecurityIdentifier(Users.Single(i => i.Name == name).SID);
            }
        }

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
        public static string LocalAppDataDir => Path.Combine(ProfilesDirectory, UserName, "AppData", "Local");

        /// <summary>
        /// Gets total installed RAM amount in GB.
        /// </summary>
        public static double MemoryTotal {
            get {
                if (_MemoryTotal > 0) return _MemoryTotal;
                var p = new IntPtr(0L);
                NativeMethods.GetPhysicallyInstalledSystemMemory(out p);
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
                var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                var key = hklm.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion");
                var value = key.GetValue("ProductId");
                return value != null ? _WindowsProductId = value.ToString() : NotAvailable;
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
                    current = current * 256;
                    current = digitalProductId[j + keyOffset] + current;
                    digitalProductId[j + keyOffset] = (byte)(current / 24);
                    current = current % 24;
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

            /// <summary>
            /// Retrieves the amount of RAM that is physically installed on the computer.
            /// </summary>
            /// <param name="totalMemoryInKilobytes">A pointer to a variable that receives the amount of physically installed RAM, in kilobytes.</param>
            /// <returns>If the function succeeds, it returns TRUE and sets the TotalMemoryInKilobytes parameter to a nonzero value.</returns>
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool GetPhysicallyInstalledSystemMemory(out IntPtr totalMemoryInKilobytes);

        }

    }

    #endregion

}