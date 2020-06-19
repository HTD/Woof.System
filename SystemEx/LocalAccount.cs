using System;
using System.Linq;
using System.Security.Principal;

using Woof.SystemEx.Win32Imports;
using Woof.SystemEx.Win32Types;

namespace Woof.SystemEx {

    /// <summary>
    /// Represents an existing local Windows account.
    /// </summary>
    public class LocalAccount {

        /// <summary>
        /// Gets the account name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Gets the Windows account FullName property if available.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the <see cref="SecurityIdentifier"/> of the accouont.
        /// </summary>
        public SecurityIdentifier Sid { get; }

        /// <summary>
        /// Gets the value indicating whether this account is a member of the local administrators group.
        /// Performs actual check.
        /// </summary>
        public bool IsAdmin => SysInfo.GetLocalGroupMembers(WellKnownSidType.BuiltinAdministratorsSid).Contains(this);

        /// <summary>
        /// Creates <see cref="LocalAccount"/> from <see cref="LocalGroupMember"/>.
        /// </summary>
        /// <param name="member">Local group member.</param>
        internal LocalAccount(LocalGroupMember member) : this(new SecurityIdentifier(member.PSid)) { }

        /// <summary>
        /// Creates <see cref="LocalAccount"/> from <see cref="UserInfo"/> object.
        /// </summary>
        /// <param name="userInfo">Data structure returned from <see cref="NativeMethods.NetUserEnum"/> call.</param>
        internal LocalAccount(UserInfo userInfo) {
            Name = userInfo.Name;
            var account = new NTAccount(Name);
            Sid = account.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
            account = Sid?.Translate(typeof(NTAccount)) as NTAccount;
            FullName = userInfo.FullName;
            var split = account.Value.Split('\\');
            Domain = split.First();
        }

        /// <summary>
        /// Creates <see cref="LocalAccount"/> from the user name (with or without domain).
        /// </summary>
        /// <param name="userName">Either domain backslash user, or just user name.</param>
        public LocalAccount(string userName) : this(SysInfo.GetSecurityIdentifier(userName, stripDomain: true)) {
            if (Sid is null) {
                var i = userName.IndexOf('\\');
                if (i < 0) {
                    Name = userName;
                }
                else {
                    Name = userName.Substring(i + 1);
                    Domain = userName.Substring(0, i);
                }
            }
        }

        /// <summary>
        /// Creates <see cref="LocalAccount"/> from the doman and name.
        /// </summary>
        /// <param name="domain">Domain.</param>
        /// <param name="name">User name.</param>
        public LocalAccount(string domain, string name) : this(SysInfo.GetSecurityIdentifier(name)) {
            if (Sid is null) {
                Name = name;
                Domain = domain;
            }
        }

        /// <summary>
        /// Creates <see cref="LocalAccount"/> from <see cref="SecurityIdentifier"/>.
        /// </summary>
        /// <param name="sid"><see cref="SecurityIdentifier"/>.</param>
        public LocalAccount(SecurityIdentifier sid) {
            if (SysInfo.GetLocalAccounts(withDisabled: true).FirstOrDefault(i => i.Sid == sid) is LocalAccount account) {
                Name = account.Name;
                Domain = account.Domain;
                FullName = account.FullName;
                Sid = sid;
            }
        }

        /// <summary>
        /// Returns string representation of the local account.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString() => $"{FullName} {{{Sid?.Value}}}";

        /// <summary>
        /// Equality test depending on SID only.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if SIDs are equal.</returns>
        public override bool Equals(object obj) => (obj is LocalAccount a) && (a?.Sid?.Equals(Sid) ?? false);

        /// <summary>
        /// Gets the hash code from SID for equality tests.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode() => Sid?.GetHashCode() ?? 0;

        /// <summary>
        /// Equality test based on SID equality only.
        /// </summary>
        /// <param name="a">This one.</param>
        /// <param name="b">That one.</param>
        /// <returns>True if SIDs are equal.</returns>
        public static bool operator ==(LocalAccount a, LocalAccount b) =>
            (a is null && b is null) || a?.Sid?.Equals(b?.Sid) == true;

        /// <summary>
        /// Inequality test based on SID inequality only.
        /// </summary>
        /// <param name="a">This one.</param>
        /// <param name="b">That one.</param>
        /// <returns>True if SIDs are NOT equal.</returns>
        public static bool operator !=(LocalAccount a, LocalAccount b) =>
            !(a is null && b is null) && a?.Sid?.Equals(b?.Sid) != true;

    }

}