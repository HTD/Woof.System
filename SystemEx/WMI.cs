using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Management; // add to references.

namespace Woof.SystemEx {

    /// <summary>
    /// Safe, managed WMI queries support.
    /// </summary>
    public static class WMI {

        /// <summary>
        /// Queries WMI and returns results as an array of dynamic objects.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static dynamic[] Query(string q) {
            using (var s = new ManagementObjectSearcher(q))
                return (from ManagementObject m in s.Get() select m.AsDynamic()).ToArray();
        }

    }

    /// <summary>
    /// <see cref="ManagementObject"/> to <see cref="ExpandoObject"/> conversion class.
    /// </summary>
    public static class ManagementObjectExtensions {

        /// <summary>
        /// Returns <see cref="ManagementObject"/> as <see cref="ExpandoObject"/>.
        /// </summary>
        /// <param name="m"><see cref="ManagementObject"/>.</param>
        /// <returns></returns>
        public static ExpandoObject AsDynamic(this ManagementObject m) {
            var x = new ExpandoObject();
            using (m) foreach (var p in m.Properties) (x as IDictionary<string, object>).Add(p.Name, p.Value);
            return x;
        }

    }

}