using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Woof.SystemEx.Tests {

    [TestClass]
    public class SysInfoTests {

        [TestMethod]
        public void Sids() {
            Console.WriteLine($"LogonUserSid = {SysInfo.LogonUserSid}");
            Console.WriteLine($"LogonUserName = {SysInfo.LogonUserName}");
            Console.WriteLine($"UserSid = {SysInfo.UserSid}");
            Console.WriteLine($"UserName = {SysInfo.UserName}");
        }

        [TestMethod]
        public void GetUserPicturePaths() {
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