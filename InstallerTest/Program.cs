using System;
using System.Windows;

using Woof.SystemEx;

namespace InstallerTest {

    class Program {

        static int Main(string[] args) {
            var exitCode = 0;
            if (args.Length == 1 && Int32.TryParse(args[0], out int code)) exitCode = code;
            var output = String.Join(Environment.NewLine, new string[] {
                $"LogonUser.Name = {SysInfo.LogonUser.Name}",
                $"LogonUser.Domain = {SysInfo.LogonUser.Domain}",
                $"LogonUser.Sid = {SysInfo.LogonUser.Sid}",
                $"LogonUser.IsAdmin = {SysInfo.LogonUser.IsAdmin}",
                $"CurrentProcessUser.Name = {SysInfo.CurrentProcessUser.Name}",
                $"CurrentProcessUser.Domain = {SysInfo.CurrentProcessUser.Domain}",
                $"CurrentProcessUser.Sid = {SysInfo.CurrentProcessUser.Sid}",
                $"CurrentProcessUser.IsAdmin = {SysInfo.CurrentProcessUser.IsAdmin}"
            });
            MessageBox.Show(output);
            return exitCode;
        }

    }

}