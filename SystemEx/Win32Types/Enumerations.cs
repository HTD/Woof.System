using System;

namespace Woof.SystemEx.Win32Types {

    /// <summary>
    /// User account types to be included in the enumeration.
    /// </summary>
    [Flags]
    internal enum NetApiFilter {

        /// <summary>
        /// Enumerates account data for users whose primary account is in another domain. This account type provides user access to this domain, but not to any domain that trusts this domain. The User Manager refers to this account type as a local user account.
        /// </summary>
        TempDuplicateAccount = 0x0001,

        /// <summary>
        /// Enumerates normal user account data. This account type is associated with a typical user.
        /// </summary>
        NormalAccount = 0x0002,

        /// <summary>
        /// Enumerates interdomain trust account data. This account type is associated with a trust account for a domain that trusts other domains.
        /// </summary>
        InterdomainTrustAccount = 0x0008,

        /// <summary>
        /// Enumerates workstation or member server trust account data. This account type is associated with a machine account for a computer that is a member of the domain.
        /// </summary>
        WorkstationTrustAccount = 0x0010,

        /// <summary>
        /// Enumerates member server machine account data. This account type is associated with a computer account for a backup domain controller that is a member of the domain.
        /// </summary>
        ServerTrustAccount = 0x0020

    }

    /// <summary>
    /// The status of the Net API function.
    /// </summary>
    internal enum NetApiStatus {

        /// <summary>
        /// Net API function succeeded.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The user does not have access to the requested information.
        /// </summary>
        AccessDenied = 5,

        /// <summary>
        /// More entries are available. Specify a large enough buffer to receive all entries.
        /// </summary>
        MoreData = 234,

        /// <summary>
        /// The return buffer is too small.
        /// </summary>
        BufferTooSmall = 2123,

        /// <summary>
        /// The computer name is invalid.
        /// </summary>
        InvalidComputer = 2351

    }

    /// <summary>
    /// The account type associated with the security identifier specified in the lgrmi2_sid member.
    /// </summary>
    [Flags]
    internal enum SidNameUse {

        /// <summary>
        /// The account is a user account.
        /// </summary>
        User = 1,

        /// <summary>
        /// The account is a global group account.
        /// </summary>
        Group,

        /// <summary>
        /// The account is a well-known group account (such as Everyone). For more information, see Well-Known SIDs.
        /// </summary>
        Domain,

        /// <summary>
        /// The account is an alias.
        /// </summary>
        Alias,

        /// <summary>
        /// The account is a well-known group account (such as Everyone). For more information, see Well-Known SIDs.
        /// </summary>
        WellKnownGroup,

        /// <summary>
        /// The account has been deleted.
        /// </summary>
        DeletedAccount,

        /// <summary>
        /// The account is invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// The account type cannot be determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The account is a computer.
        /// </summary>
        Computer

    }

    /// <summary>
    /// Flags for user accounts.
    /// </summary>
    [Flags]
    internal enum UserFlags {

        /// <summary>
        /// This is an account for users whose primary account is in another domain. This account provides user access to this domain, but not to any domain that trusts this domain. The User Manager refers to this account type as a local user account.
        /// </summary>
        TempDuplicateAccount = 0x0100,

        /// <summary>
        /// This is a default account type that represents a typical user.
        /// </summary>
        NormalAccount = 0x0200,

        /// <summary>
        /// This is a permit to trust account for a domain that trusts other domains.
        /// </summary>
        InterdomainTrustAccount = 0x0800,

        /// <summary>
        /// This is a computer account for a computer that is a member of this domain.
        /// </summary>
        WorkstationTrustAccount = 0x1000,

        /// <summary>
        /// This is a computer account for a backup domain controller that is a member of this domain.
        /// </summary>
        ServerTrustAccount = 0x2000,

        /// <summary>
        /// The password should never expire on the account.
        /// </summary>
        DontExpirePassword = 0x10000,

        /// <summary>
        /// The logon script executed. This value must be set.
        /// </summary>
        Script = 0x0001,

        /// <summary>
        /// The user's account is disabled.
        /// </summary>
        AccountDisable = 0x0002,

        /// <summary>
        /// The home directory is required. This value is ignored.
        /// </summary>
        HomeDirRequired = 0x0008,

        /// <summary>
        /// No password is required.
        /// </summary>
        PasswordNotRequired = 0x0020,

        /// <summary>
        /// The user cannot change the password.
        /// </summary>
        PasswordCantChange = 0x0040,

        /// <summary>
        /// The account is currently locked out. You can call the NetUserSetInfo function to clear this value and unlock a previously locked account. You cannot use this value to lock a previously unlocked account.
        /// </summary>
        AccountLockout = 0x0010,

        /// <summary>
        /// The user's password is stored under reversible encryption in the Active Directory.
        /// </summary>
        EncryptedTextPasswordAllowed = 0x0080,

        /// <summary>
        /// The user's password has expired.
        /// </summary>
        UserPasswordExpired = 0x800000

    }

}