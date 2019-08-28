using System;
using System.Linq;
using System.Security.Principal;
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
        /// Gets the domain name backslash account name.
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
        internal LocalAccount(LocalGroupMember member) {
            FullName = member.DomainAndName;
            Sid = new SecurityIdentifier(member.PSid);
            var split = FullName.Split('\\');
            Domain = split.First();
            Name = split.Last();
        }

        internal LocalAccount(UserInfo userInfo) {
            Name = userInfo.Name;
            var account = new NTAccount(Name);
            Sid = account.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
            account = Sid.Translate(typeof(NTAccount)) as NTAccount;
            FullName = account.Value;
            var split = FullName.Split('\\');
            Domain = split.First();
        }

        /// <summary>
        /// Creates <see cref="LocalAccount"/> from the full name (with domain).
        /// </summary>
        /// <param name="domainAndName">Domain backslash user name.</param>
        public LocalAccount(string domainAndName) {
            FullName = domainAndName;
            var split = FullName.Split('\\');
            Domain = split.First();
            Name = split.Last();
            Sid = SysInfo.GetSecurityIdentifier(Name);
        }

        /// <summary>
        /// Creates <see cref="LocalAccount"/> from the doman and name.
        /// </summary>
        /// <param name="domain">Domain.</param>
        /// <param name="name">User name.</param>
        public LocalAccount(string domain, string name) {
            Domain = domain;
            Name = name;
            FullName = $"{domain}\\{name}";
            Sid = SysInfo.GetSecurityIdentifier(Name);
        }

        /// <summary>
        /// Creates <see cref="LocalAccount"/> from <see cref="SecurityIdentifier"/>.
        /// </summary>
        /// <param name="sid"><see cref="SecurityIdentifier"/>.</param>
        public LocalAccount(SecurityIdentifier sid) {
            Sid = sid;
            FullName = Sid.Translate(typeof(NTAccount)).Value;
            var split = FullName.Split('\\');
            Domain = split.First();
            Name = split.Last();
        }

        /// <summary>
        /// Equality test depending on SID only.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if SIDs are equal.</returns>
        public override bool Equals(object obj) => (obj is LocalAccount a) && a.Sid.Equals(Sid);

        /// <summary>
        /// Gets the hash code from SID for equality tests.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode() => Sid.GetHashCode();

        /// <summary>
        /// Equality test based on SID equality only.
        /// </summary>
        /// <param name="a">This one.</param>
        /// <param name="b">That one.</param>
        /// <returns>True if SIDs are equal.</returns>
        public static bool operator ==(LocalAccount a, LocalAccount b) =>
            (a is null && b is null) || a?.Sid.Equals(b?.Sid) == true;

        /// <summary>
        /// Inequality test based on SID inequality only.
        /// </summary>
        /// <param name="a">This one.</param>
        /// <param name="b">That one.</param>
        /// <returns>True if SIDs are NOT equal.</returns>
        public static bool operator !=(LocalAccount a, LocalAccount b) =>
            !(a is null && b is null) && a?.Sid.Equals(b?.Sid) != true;

    }

}