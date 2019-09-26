using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Woof.SystemEx;

namespace Woof.System.WhoAmITest {
    class Program {
        static void Main() => Console.WriteLine(SysInfo.LogonUser);
    }
}
