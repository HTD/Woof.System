using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Woof.SystemEx.Tests {

    [TestClass]
    public class SysInfoTests {

        [TestMethod]
        public void DeviceInfo() {
            Console.WriteLine($"CPU:");
            foreach (var p in SysInfo.Cpu) Console.WriteLine($" - {p.Key} = {p.Value}");
            Console.WriteLine($"CpuId = {SysInfo.CpuId}");
            Console.WriteLine($"CpuCores = {SysInfo.CpuCores}");
            Console.WriteLine($"CpuCount = {SysInfo.CpuCount}");
            Console.WriteLine($"CpuSpeed = {SysInfo.CpuSpeed}");
            Console.WriteLine($"LogonUserFullName = {SysInfo.LogonUserFullName}");
            Console.WriteLine($"LogonUserDomain = {SysInfo.LogonUserDomain}");
            Console.WriteLine($"LogonUserName = {SysInfo.LogonUserName}");
            Console.WriteLine($"IsLogonUserAdmin = {SysInfo.IsLogonUserAdmin}");
            Console.WriteLine($"LogonUserSid = {SysInfo.LogonUserSid}");
            Console.WriteLine($"UserSid = {SysInfo.UserSid}");
            Console.WriteLine($"UserName = {SysInfo.UserName}");
            Console.WriteLine($"DeviceId = {SysInfo.DeviceId}");
            Console.WriteLine($"IMEI = {SysInfo.IMEI}");
            Console.WriteLine($"LocalAppDataDir = {SysInfo.LocalAppDataDir}");
            Console.WriteLine($"MemoryTotal = {SysInfo.MemoryTotal}");
            Console.WriteLine("PhysicalMacs:");
            foreach (var mac in SysInfo.PhysicalMacs) Console.WriteLine($" - {mac}");
            Console.WriteLine($"ProfilesDirectory = {SysInfo.ProfilesDirectory}");
            Console.WriteLine($"SystemDiskSerialNumber = {SysInfo.SystemDiskSerialNumber}");
            Console.WriteLine($"SystemMemoryFree = {SysInfo.SystemMemoryFree}");
            Console.WriteLine($"SystemMemoryTotal = {SysInfo.SystemMemoryTotal}");
            Console.WriteLine("Users:");
            foreach (var user in SysInfo.Users) Console.WriteLine($" - {user.SID} => {user.Name}");
            Console.WriteLine($"WindowsProductId = {SysInfo.WindowsProductId}");
            Console.WriteLine($"WindowsProductKey = {SysInfo.WindowsProductKey}");
        }

        [TestMethod]
        public void SidsAndNames() {
            Console.WriteLine($"LogonUserSid = {SysInfo.LogonUserSid}");
            Console.WriteLine($"LogonUserName = {SysInfo.LogonUserName}");
            Console.WriteLine($"UserSid = {SysInfo.UserSid}");
            Console.WriteLine($"UserName = {SysInfo.UserName}");
        }

        [TestMethod]
        public void UserAttributes() {
            ExpandoObject user = SysInfo.Users.First();
            foreach (var p in user) Console.WriteLine($"{p.Key} = {p.Value}");
        }

        [TestMethod]
        public void UserPicturePaths() {
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

}