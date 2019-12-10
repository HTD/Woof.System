using MbnApi; // add to references from: C:\Program Files (x86)\Windows Kits\10\Lib\{Build}\um\x86, requires Windows 10 SDK.
using Microsoft.VisualBasic.Devices; // add to references.
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess; // add to references.
using System.Text;
using Woof.SystemEx.Win32Types;
using Woof.SystemEx.Win32Imports;

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
        /// Gets IMEI (International Mobile Equipment Identity) for mobile devices, null for dekstops.
        /// </summary>
        public static string IMEI => _IMEI ?? (_IMEI = GetIMEI());


        /// <summary>
        /// Gets unique device identifier based on mobile broadband network interface IMEI, system disk serial number or concatenation of physical networ adapters MAC addresses.
        /// </summary>
        public static Guid DeviceId => _DeviceId ?? (_DeviceId = new DGuid(IMEI ?? SystemDiskSerialNumber ?? String.Join(" ", PhysicalMacs)));

        /// <summary>
        /// Gets the current process user.
        /// </summary>
        public static LocalAccount CurrentProcessUser {
            get {
                using (var currentUserIdentity = WindowsIdentity.GetCurrent())
                    return new LocalAccount(currentUserIdentity.User);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current process user has actual administrative privileges.
        /// </summary>
        public static bool IsCurrentProcessUserAdmin {
            get {
                using (var currentUserIdentity = WindowsIdentity.GetCurrent())
                    return currentUserIdentity.Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        /// <summary>
        /// Gets the user account who actually owns the active UI session.
        /// </summary>
        public static LocalAccount LogonUser {
            get {
                using (var currentUserIdentity = WindowsIdentity.GetCurrent()) {
                    if (currentUserIdentity.IsSystem) {
                        IntPtr userToken = IntPtr.Zero;
                        try {
                            userToken = GetSessionUserToken();
                            if (userToken != IntPtr.Zero)
                                using (var tokenUserIdentity = new WindowsIdentity(userToken))
                                    return new LocalAccount(tokenUserIdentity.User);
                            else return new LocalAccount(currentUserIdentity.User);
                        }
                        catch {
                            return new LocalAccount(currentUserIdentity.User);
                        }
                        finally {
                            if (userToken != IntPtr.Zero) NativeMethods.CloseHandle(userToken);
                        }
                    }
                    else return new LocalAccount(currentUserIdentity.User);
                }
            }

        }

        /// <summary>
        /// Gets the local groups.
        /// </summary>
        public static IEnumerable<LocalGroup> LocalGroups => GetLocalGroups();

        /// <summary>
        /// Gets local AppData directory for current user, WORKS FROM SYSTEM ACCOUNT!
        /// </summary>
        public static string LocalAppDataDirectory
            => _LocalAppDataDirectory ?? (_LocalAppDataDirectory = Path.Combine(ProfilesDirectory, LogonUser.Name, "AppData", "Local"));

        /// <summary>
        /// Gets user profiles directory from Windows registry.
        /// </summary>
        public static string ProfilesDirectory
            => _ProfilesDirectory ?? (_ProfilesDirectory = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList", "ProfilesDirectory", ""));

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
        public static IEnumerable<string> PhysicalMacs => WMI.Query("SELECT MACAddress FROM Win32_NetworkAdapterConfiguration WHERE MACAddress IS NOT NULL AND NOT Description LIKE \"%Virtual%\"").Select(i => (string)i.MACAddress);

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
        /// Gets all normal, enabled user accounts on the local computer.
        /// </summary>
        public static IEnumerable<LocalAccount> Users => GetLocalAccounts();

        /// <summary>
        /// Gets the operating system extended information from WMI.
        /// </summary>
        public static dynamic OperatingSystem
            => _OperatingSystem ?? (_OperatingSystem = WMI.Query("SELECT * FROM Win32_OperatingSystem").First());

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
        /// Gets the account of the process owner.
        /// </summary>
        /// <param name="process">Process.</param>
        /// <returns>Account information.</returns>
        public static LocalAccount GetProcessUser(Process process) {
            IntPtr processHandle = IntPtr.Zero;
            try {
                
                NativeMethods.OpenProcessToken(process.Handle, TokenAccessLevels.Query, out processHandle);
                using (var identity = new WindowsIdentity(processHandle)) return new LocalAccount(identity.User);
            }
            finally {
                if (processHandle != IntPtr.Zero) NativeMethods.CloseHandle(processHandle);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the process is elevated.
        /// </summary>
        /// <param name="process">Process.</param>
        /// <returns>True if elevated.</returns>
        public static bool GetProcessElevated(Process process) {
            IntPtr processHandle = IntPtr.Zero;
            try {

                NativeMethods.OpenProcessToken(process.Handle, TokenAccessLevels.Query, out processHandle);
                var elevationResult = TokenElevationType.Default;
                var elevationResultSize = Marshal.SizeOf((int)elevationResult);
                var elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);
                try {
                    var success = NativeMethods.GetTokenInformation(processHandle, TokenInformationClass.TokenElevationType, elevationTypePtr, (uint)elevationResultSize, out var returnedSize);
                    if (success) {
                        elevationResult = (TokenElevationType)Marshal.ReadInt32(elevationTypePtr);
                        return elevationResult == TokenElevationType.Full;
                    }
                    return false;
                }
                finally {
                    if (elevationTypePtr != IntPtr.Zero) Marshal.FreeHGlobal(elevationTypePtr);
                }
            }
            finally {
                if (processHandle != IntPtr.Zero) NativeMethods.CloseHandle(processHandle);
            }
        }

        /// <summary>
        /// Gets the <see cref="SecurityIdentifier"/> for the user name in the system.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="stripDomain">Set true to strip the domain from the user name. Default false.</param>
        /// <returns>SID or null if the user name does not exist in the system.</returns>
        public static SecurityIdentifier GetSecurityIdentifier(string userName, bool stripDomain = false) {
            if (stripDomain) {
                var p = userName.IndexOf('\\');
                if (p >= 0) userName = userName.Substring(p + 1);
            }
            try {
                return new NTAccount(userName).Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
            }
            catch (IdentityNotMappedException) {
                return null;
            }
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
            NativeMethods.SHGetUserPicturePath(msAccountName ?? userName, GetUserPictureFlags.CreatePicturesDir, pathBuffer, pathBuffer.Capacity);
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
            NativeMethods.SHGetUserPicturePathEx(msAccountName ?? userName, GetUserPictureFlags.CreatePicturesDir, null, pathBuffer, pathBuffer.Capacity, srcBuffer, srcBuffer.Capacity);
            srcPath = srcBuffer.ToString();
            if (srcPath.StartsWith("\\\\")) { // converts machine-name format to standard windows path
                srcPath = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2) + srcPath.Substring(srcPath.IndexOf('\\', 2));
            }
            return pathBuffer.ToString();
        }


        #region Helpers

        #region Visual Basic

        /// <summary>
        /// Gets cached <see cref="Microsoft.VisualBasic.Devices.ComputerInfo"/> instance.
        /// </summary>
        private static ComputerInfo ComputerInfo => _ComputerInfo ?? (_ComputerInfo = new ComputerInfo());

        #endregion

        #region MBN API

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

        #endregion

        #region Product key

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

        #endregion

        #region Net API

        /// <summary>
        /// Reads data from the Net API.
        /// </summary>
        /// <typeparam name="T">Type of the structure to read.</typeparam>
        /// <param name="status">One of <see cref="NetApiStatus"/> enumeration.</param>
        /// <param name="buffer">Pointer to the buffer returned by Net API.</param>
        /// <param name="read">The number of elements read by Net API.</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        internal static IEnumerable<T> ReadNetApi<T>(NetApiStatus status, IntPtr buffer, int read) where T : struct {
            if (status != NetApiStatus.Success) {
                if (status == NetApiStatus.AccessDenied) throw new UnauthorizedAccessException();
                else throw new InvalidOperationException($"NetApi {status}");
            }
            var location = buffer;
            for (int i = 0; i < read; i++) {
                yield return (T)Marshal.PtrToStructure(location, typeof(T));
                location += Marshal.SizeOf<T>();
            }


        }

        /// <summary>
        /// Gets all accounts. Full information, with FullName set.
        /// </summary>
        /// <param name="withDisabled">Set true to include disabled accounts. Default false.</param>
        /// <returns>Accounts.</returns>
        internal static LocalAccount[] GetLocalAccounts(bool withDisabled = false) {
            var buffer = IntPtr.Zero;
            try {
                var status = NativeMethods.NetUserEnum(null, 20, NetApiFilter.NormalAccount, ref buffer, -1, out var read, out var total, IntPtr.Zero);
                var accounts = ReadNetApi<UserInfo>(status, buffer, read);
                var normalAccounts = accounts.Where(i => i.Flags.HasFlag(UserFlags.NormalAccount) && !i.Name.EndsWith("$", StringComparison.Ordinal));
                return withDisabled
                    ? normalAccounts.Select(i => new LocalAccount(i)).ToArray()
                    : normalAccounts.Where(i => !i.Flags.HasFlag(UserFlags.AccountDisable)).Select(i => new LocalAccount(i)).ToArray();
            }
            finally {
                if (buffer != IntPtr.Zero) NativeMethods.NetApiBufferFree(buffer);
            }
        }

        /// <summary>
        /// Gets the local accounts within a group.
        /// </summary>
        /// <param name="localizedGroupName">Localized group name.</param>
        /// <returns>Local accounts.</returns>
        internal static LocalAccount[] GetLocalAccounts(string localizedGroupName) {
            var buffer = IntPtr.Zero;
            try {
                var status = NativeMethods.NetLocalGroupGetMembers(null, localizedGroupName, 2, ref buffer, -1, out var read, out var total, IntPtr.Zero);
                return ReadNetApi<LocalGroupMember>(status, buffer, read)
                    .Where(i => i.SidUsage.HasFlag(SidNameUse.User))
                    .Select(i => new LocalAccount(i)).ToArray();
            }
            finally {
                if (buffer != IntPtr.Zero) NativeMethods.NetApiBufferFree(buffer);
            }
        }

        /// <summary>
        /// Gets the local groups of the local computer.
        /// </summary>
        /// <returns>Local groups.</returns>
        internal static LocalGroup[] GetLocalGroups() {
            var buffer = IntPtr.Zero;
            try {
                var status = NativeMethods.NetLocalGroupEnum(null, 1, ref buffer, -1, out var read, out var total, IntPtr.Zero);
                return ReadNetApi<LocalGroupInfo>(status, buffer, read).Select(i => new LocalGroup(i)).ToArray();
            }
            finally {
                if (buffer != IntPtr.Zero) NativeMethods.NetApiBufferFree(buffer);
            }
        }

        internal static LocalAccount[] GetLocalGroupMembers(SecurityIdentifier sid) {
            var account = sid.Translate(typeof(NTAccount)) as NTAccount;
            var localizedWithDomain = account.Value;
            var localized = localizedWithDomain.Split('\\').Last();
            return GetLocalAccounts(localized);
        }

        internal static LocalAccount[] GetLocalGroupMembers(WellKnownSidType sidType)
            => GetLocalGroupMembers(new SecurityIdentifier(sidType, null));


        #endregion

        #region WTS API

        const uint WtsApiInvalidSessionId = 0xFFFFFFFF;

        /// <summary>
        /// Gets the user impersonation token from the currently active session.
        /// Do not invoke as regular user, the proper usage is from system account.
        /// </summary>
        /// <returns>Impersonation token for the active session owner.</returns>
        private static IntPtr GetSessionUserToken() {
            var wtsCurrentServerHandle = IntPtr.Zero;
            var bResult = false;
            var hImpersonationToken = IntPtr.Zero;
            var activeSessionId = WtsApiInvalidSessionId;
            var pSessionInfo = IntPtr.Zero;
            var sessionCount = 0;
            var sessionUserToken = IntPtr.Zero;
            // Get a handle to the user access token for the current active session.
            if (NativeMethods.WTSEnumerateSessions(wtsCurrentServerHandle, 0, 1, ref pSessionInfo, ref sessionCount) != 0) {
                var arrayElementSize = Marshal.SizeOf(typeof(WtsSessionInfo));
                var current = (long)pSessionInfo;
                for (var i = 0; i < sessionCount; i++) {
                    var si = (WtsSessionInfo)Marshal.PtrToStructure((IntPtr)current, typeof(WtsSessionInfo));
                    current += (long)arrayElementSize;
                    if (si.State == WtsConnectState.Active) activeSessionId = si.SessionID;
                }
            }
            // If enumerating did not work, fall back to the old method
            if (activeSessionId == WtsApiInvalidSessionId) activeSessionId = NativeMethods.WTSGetActiveConsoleSessionId();
            if (NativeMethods.WTSQueryUserToken(activeSessionId, ref hImpersonationToken) != 0) {
                // Convert the impersonation token to a primary token
                bResult = NativeMethods.DuplicateTokenEx(
                    hImpersonationToken,
                    0,
                    IntPtr.Zero,
                    (int)SecurityImpersonationLevel.SecurityImpersonation,
                    (int)TokenType.TokenPrimary,
                    ref sessionUserToken);

                NativeMethods.CloseHandle(hImpersonationToken);
            }
            return bResult ? sessionUserToken : IntPtr.Zero;
        }

        #endregion

        #endregion

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
        static string _LocalAppDataDirectory;
        static dynamic _OperatingSystem;
        static double _SystemMemoryTotal;
        static byte[] _WindowsDigitalProductId;
        static string _WindowsProductId;
        static string _WindowsProductKey;

        #endregion

    }

}