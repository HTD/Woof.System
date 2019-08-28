using System.Security.Principal;

using Woof.SystemEx.Win32Types;

namespace Woof.SystemEx {

    /// <summary>
    /// Contains local group basic informations.
    /// </summary>
    public class LocalGroup {

        /// <summary>
        /// Gets the localized group name.
        /// </summary>
        public string LocalizedName { get; }

        /// <summary>
        /// Gets tje localized group description.
        /// </summary>
        public string LocalizedDescription { get; }

        /// <summary>
        /// Gets the group's <see cref="SecurityIdentifier"/>.
        /// </summary>
        public SecurityIdentifier Sid { get; }

        /// <summary>
        /// Creates <see cref="LocalGroup"/> from <see cref="LocalGroupInfo"/>.
        /// </summary>
        /// <param name="groupInfo">Value read from Net API.</param>
        internal LocalGroup(LocalGroupInfo groupInfo) {
            LocalizedName = groupInfo.Name;
            LocalizedDescription = groupInfo.Comment;
            Sid = new NTAccount(LocalizedName).Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
        }

    }

}