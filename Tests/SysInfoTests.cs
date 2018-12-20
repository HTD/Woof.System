using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
    }

}