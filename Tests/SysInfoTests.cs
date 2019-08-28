using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Woof.SystemEx.Tests {

    [TestClass]
    public class SysInfoTests {

        [TestMethod]
        public void A01_DeviceInfo() {
            Console.WriteLine($"CPU:");
            foreach (var p in SysInfo.Cpu) Console.WriteLine($" - {p.Key} = {p.Value}");
            Console.WriteLine($"CpuId = {SysInfo.CpuId}");
            Console.WriteLine($"CpuCores = {SysInfo.CpuCores}");
            Console.WriteLine($"CpuCount = {SysInfo.CpuCount}");
            Console.WriteLine($"CpuSpeed = {SysInfo.CpuSpeed}");
            Console.WriteLine($"DeviceId = {SysInfo.DeviceId}");
            Console.WriteLine($"IMEI = {SysInfo.IMEI}");
            Console.WriteLine($"MemoryTotal = {SysInfo.MemoryTotal}");
            Console.WriteLine("PhysicalMacs:");
            foreach (var mac in SysInfo.PhysicalMacs) Console.WriteLine($" - {mac}");
            Console.WriteLine($"SystemDiskSerialNumber = {SysInfo.SystemDiskSerialNumber}");
            Console.WriteLine($"SystemMemoryFree = {SysInfo.SystemMemoryFree}");
            Console.WriteLine($"SystemMemoryTotal = {SysInfo.SystemMemoryTotal}");
            Console.WriteLine($"WindowsProductId = {SysInfo.WindowsProductId}");
            Console.WriteLine($"WindowsProductKey = {SysInfo.WindowsProductKey}");
        }

        [TestMethod]
        public void A02_Users() {
            Console.WriteLine("Users:");
            foreach (var user in SysInfo.Users) Console.WriteLine($"  [{user.Sid}] => {user.Name} [{(user.IsAdmin ? "admin" : "user")}]");
            Console.WriteLine();
            Console.WriteLine($"LogonUser.Sid = {SysInfo.LogonUser.Sid}");
            Console.WriteLine($"LogonUser.Name = {SysInfo.LogonUser.Name}");
            Console.WriteLine($"LogonUser.Domain = {SysInfo.LogonUser.Domain}");
            Console.WriteLine($"LogonUser.FullName = {SysInfo.LogonUser.FullName}");
            Console.WriteLine($"LogonUser.IsAdmin = {SysInfo.LogonUser.IsAdmin}");
            Console.WriteLine();
            Console.WriteLine($"ProfilesDirectory = {SysInfo.ProfilesDirectory}");
            Console.WriteLine($"LocalAppDataDirectory = {SysInfo.LocalAppDataDirectory}");
            Console.WriteLine();
            Console.WriteLine($"CurrentProcessUser.Sid = {SysInfo.CurrentProcessUser.Sid}");
            Console.WriteLine($"CurrentProcessUser.Name = {SysInfo.CurrentProcessUser.Name}");
            Console.WriteLine($"CurrentProcessUser.Domain = {SysInfo.CurrentProcessUser.Domain}");
            Console.WriteLine($"CurrentProcessUser.FullName = {SysInfo.CurrentProcessUser.FullName}");
            Console.WriteLine($"CurrentProcessUser.IsAdmin = {SysInfo.CurrentProcessUser.IsAdmin}");
        }

        [TestMethod]
        public void A03_UsersWMI() {
            Console.WriteLine("UsersWMI:");
            foreach (var user in SysInfo.UsersWMI) {
                Console.WriteLine();
                Console.WriteLine($"  {user.Name}:");
                foreach (var p in user) Console.WriteLine($"    {p.Key} = {p.Value}");
            }
            Console.WriteLine();
        }

        [TestMethod]
        public void A04_UserPicturePaths() {
            for (int i = 0; i < 10; i++) {
                Console.WriteLine($"Iteration {i + 1}:");
                var currentUserSmall1 = SysInfo.GetUserPicturePath();
                var currentUserSmall2 = SysInfo.GetUserPicturePath(null, out var currentUserLarge);
                var unknownUserSmall1 = SysInfo.GetUserPicturePath("Unknown");
                var unknownUserSmall2 = SysInfo.GetUserPicturePath("Unknown", out var unknownUserLarge);
                if (!File.Exists(currentUserSmall1)) throw new FileNotFoundException("Small image for the current user doesn't exist");
                if (!File.Exists(currentUserSmall2)) throw new FileNotFoundException("Small image for the current user doesn't exist");
                if (!File.Exists(currentUserLarge)) throw new FileNotFoundException("Large image for the current user doesn't exist");
                if (!File.Exists(unknownUserSmall1)) throw new FileNotFoundException("Small image for the unknown user doesn't exist");
                if (!File.Exists(unknownUserSmall2)) throw new FileNotFoundException("Small image for the unknown user doesn't exist");
                if (!File.Exists(unknownUserLarge)) throw new FileNotFoundException("Large image for the unknown user doesn't exist");
                Console.WriteLine($"currentUserSmall1 = {currentUserSmall1}");
                Console.WriteLine($"currentUserSmall2 = {currentUserSmall2}");
                Console.WriteLine($"currentUserLarge = {currentUserLarge}");
                Console.WriteLine($"unknownUserSmall1 = {unknownUserSmall1}");
                Console.WriteLine($"unknownUserSmall2 = {unknownUserSmall2}");
                Console.WriteLine($"unknownUserLarge = {unknownUserLarge}");
            }
        }

        [TestMethod]
        public void A05_ProcessExtendedInformation() {
            if (SysInfo.IsCurrentProcessUserAdmin) {
                var dwmProcess = Process.GetProcessesByName("dwm").FirstOrDefault();
                var dwmUser = SysInfo.GetProcessUser(dwmProcess);
                var isDwmProcessElevated = SysInfo.GetProcessElevated(dwmProcess);
                var isCurrentProcessElevated = SysInfo.GetProcessElevated(Process.GetCurrentProcess());
                Console.WriteLine();
                Console.WriteLine($"DWM process user SID = {dwmUser.Sid}");
                Console.WriteLine($"DWM process isElevated = {isDwmProcessElevated}");
                Console.WriteLine($"IsCurrentProcessElevated = {isCurrentProcessElevated}");
            }
            else Console.WriteLine("Access denied. Try again with the process elevated.");
        }

    }

}