using System;
using System.Windows;
using Woof.SystemEx;
using System.Linq;
using System.Text;
using System.Collections;

namespace InstallerTest {

    class Program {

        static int Main(string[] args) {
            var exitCode = 0;
            if (args.Length == 1 && Int32.TryParse(args[0], out int code)) exitCode = code;
            //var envStringBuilder = new StringBuilder();
            //foreach (DictionaryEntry pair in Environment.GetEnvironmentVariables())
            //    envStringBuilder.AppendLine($"{pair.Key} = {pair.Value}");
            var output = String.Join(Environment.NewLine, new string[] {
                $"LogonUserName = {SysInfo.LogonUserName}",
                $"LogonUserDomain = {SysInfo.LogonUserDomain}",
                $"LogonUserSid = {SysInfo.LogonUserSid}",
                $"UserSid = {SysInfo.UserSid}",
                $"UserSid.IsAccountSid() = {SysInfo.UserSid.IsAccountSid()}",
                $"UserName = {SysInfo.UserName}",
                $"IsLogonUserAdmin = {SysInfo.IsLogonUserAdmin}"
            });
            MessageBox.Show(output);
            return exitCode;
        }

    }

}