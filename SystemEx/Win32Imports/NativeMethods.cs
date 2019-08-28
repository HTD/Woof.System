using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

using Woof.SystemEx.Win32Types;

namespace Woof.SystemEx.Win32Imports {

    internal static class NativeMethods {

        #region Net Api

        /// <summary>
        /// The NetApiBufferFree function frees the memory that the NetApiBufferAllocate function allocates. Applications should also call NetApiBufferFree to free the memory that other network management functions use internally to return information.
        /// </summary>
        /// <param name="bufptr">A pointer to a buffer returned previously by another network management function or memory allocated by calling the NetApiBufferAllocate function.</param>
        [DllImport("netapi32.dll", EntryPoint = "NetApiBufferFree")]
        internal static extern void NetApiBufferFree(IntPtr bufptr);

        /// <summary>
        /// Returns information about each local group account on the specified server.
        /// </summary>
        /// <param name="servername">Pointer to a constant string that specifies the DNS or NetBIOS name of the remote server on which the function is to execute. If this parameter is NULL, the local computer is used.</param>
        /// <param name="level">
        /// 0 : Return local group names.The bufptr parameter points to an array of LOCALGROUP_INFO_0 structures.<br/>
        /// 1 : Return local group names and the comment associated with each group. The bufptr parameter points to an array of LOCALGROUP_INFO_1 structures.
        /// </param>
        /// <param name="bufptr">Pointer to the address of the buffer that receives the information structure. The format of this data depends on the value of the level parameter. This buffer is allocated by the system and must be freed using the NetApiBufferFree function. Note that you must free the buffer even if the function fails with ERROR_MORE_DATA.</param>
        /// <param name="prefmaxlen">Specifies the preferred maximum length of returned data, in bytes. If you specify MAX_PREFERRED_LENGTH, the function allocates the amount of memory required for the data. If you specify another value in this parameter, it can restrict the number of bytes that the function returns. If the buffer size is insufficient to hold all entries, the function returns ERROR_MORE_DATA. For more information, see Network Management Function Buffers and Network Management Function Buffer Lengths.</param>
        /// <param name="entriesread">Pointer to a value that receives the count of elements actually enumerated.</param>
        /// <param name="totalentries">Pointer to a value that receives the approximate total number of entries that could have been enumerated from the current resume position. The total number of entries is only a hint. For more information about determining the exact number of entries, see the following Remarks section.</param>
        /// <param name="resumeHandle">Pointer to a value that contains a resume handle that is used to continue an existing local group search. The handle should be zero on the first call and left unchanged for subsequent calls. If this parameter is NULL, then no resume handle is stored. For more information, see the following Remarks section.</param>
        /// <returns>If the function succeeds, the return value is NERR_Success.</returns>
        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        internal extern static NetApiStatus NetLocalGroupEnum(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            int level,
            ref IntPtr bufptr,
            int prefmaxlen,
            out int entriesread,
            out int totalentries,
            IntPtr resumeHandle);

        /// <summary>
        /// Retrieves a list of the members of a particular local group in the security database, which is the security accounts manager (SAM) database or, in the case of domain controllers, the Active Directory. Local group members can be users or global groups.
        /// </summary>
        /// <param name="servername">Pointer to a constant string that specifies the DNS or NetBIOS name of the remote server on which the function is to execute. If this parameter is NULL, the local computer is used.</param>
        /// <param name="localgroupname">Pointer to a constant string that specifies the name of the local group whose members are to be listed. For more information, see the following Remarks section.</param>
        /// <param name="level">
        /// 0 : Return the security identifier (SID) associated with the local group member. The bufptr parameter points to an array of LOCALGROUP_MEMBERS_INFO_0 structures.<br/>
        /// 1 : Return the SID and account information associated with the local group member. The bufptr parameter points to an array of LOCALGROUP_MEMBERS_INFO_1 structures.<br/>
        /// 2 : Return the SID, account information, and the domain name associated with the local group member. The bufptr parameter points to an array of LOCALGROUP_MEMBERS_INFO_2 structures.<br/>
        /// 3 : Return the account and domain names of the local group member. The bufptr parameter points to an array of LOCALGROUP_MEMBERS_INFO_3 structures.
        /// </param>
        /// <param name="bufptr">Pointer to the address that receives the return information structure. The format of this data depends on the value of the level parameter. This buffer is allocated by the system and must be freed using the NetApiBufferFree function. Note that you must free the buffer even if the function fails with ERROR_MORE_DATA.</param>
        /// <param name="prefmaxlen">Specifies the preferred maximum length of returned data, in bytes. If you specify MAX_PREFERRED_LENGTH, the function allocates the amount of memory required for the data. If you specify another value in this parameter, it can restrict the number of bytes that the function returns. If the buffer size is insufficient to hold all entries, the function returns ERROR_MORE_DATA. For more information, see Network Management Function Buffers and Network Management Function Buffer Lengths.</param>
        /// <param name="entriesread">Pointer to a value that receives the count of elements actually enumerated.</param>
        /// <param name="totalentries">Pointer to a value that receives the total number of entries that could have been enumerated from the current resume position.</param>
        /// <param name="resumeHandle">Pointer to a value that contains a resume handle which is used to continue an existing group member search. The handle should be zero on the first call and left unchanged for subsequent calls. If this parameter is NULL, then no resume handle is stored.</param>
        /// <returns>If the function succeeds, the return value is NERR_Success.</returns>
        [DllImport("NetAPI32.dll", CharSet = CharSet.Unicode)]
        internal extern static NetApiStatus NetLocalGroupGetMembers(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            [MarshalAs(UnmanagedType.LPWStr)] string localgroupname,
            int level,
            ref IntPtr bufptr,
            int prefmaxlen,
            out int entriesread,
            out int totalentries,
            IntPtr resumeHandle);

        /// <summary>
        /// Retrieves information about all user accounts on a server.
        /// </summary>
        /// <param name="servername">A pointer to a constant string that specifies the DNS or NetBIOS name of the remote server on which the function is to execute. If this parameter is NULL, the local computer is used.</param>
        /// <param name="level">
        /// 0 : Return user account names. The bufptr parameter points to an array of USER_INFO_0 structures.<br/>
        /// 1 : Return detailed information about user accounts. The bufptr parameter points to an array of USER_INFO_1 structures.<br/>
        /// 2 : Return detailed information about user accounts, including authorization levels and logon information. The bufptr parameter points to an array of USER_INFO_2 structures.<br/>
        /// 3 : Return detailed information about user accounts, including authorization levels, logon information, RIDs for the user and the primary group, and profile information. The bufptr parameter points to an array of USER_INFO_3 structures.<br/>
        /// 10 : Return user and account names and comments. The bufptr parameter points to an array of USER_INFO_10 structures.<br/>
        /// 11 : Return detailed information about user accounts. The bufptr parameter points to an array of USER_INFO_11 structures.<br/>
        /// 20 : Return the user's name and identifier and various account attributes. The bufptr parameter points to an array of USER_INFO_20 structures. Note that on Windows XP and later, it is recommended that you use USER_INFO_23 instead.
        /// </param>
        /// <param name="filter">A value that specifies the user account types to be included in the enumeration. A value of zero indicates that all normal user, trust data, and machine account data should be included.<br/>
        /// FILTER_TEMP_DUPLICATE_ACCOUNT : Enumerates account data for users whose primary account is in another domain.This account type provides user access to this domain, but not to any domain that trusts this domain.The User Manager refers to this account type as a local user account.<br/>
        /// FILTER_NORMAL_ACCOUNT : Enumerates normal user account data. This account type is associated with a typical user.<br/>
        /// FILTER_INTERDOMAIN_TRUST_ACCOUNT : Enumerates interdomain trust account data. This account type is associated with a trust account for a domain that trusts other domains.<br/>
        /// FILTER_WORKSTATION_TRUST_ACCOUNT : Enumerates workstation or member server trust account data. This account type is associated with a machine account for a computer that is a member of the domain.<br/>
        /// FILTER_SERVER_TRUST_ACCOUNT : Enumerates member server machine account data. This account type is associated with a computer account for a backup domain controller that is a member of the domain.
        /// </param>
        /// <param name="bufptr">A pointer to the buffer that receives the data. The format of this data depends on the value of the level parameter.</param>
        /// <param name="prefmaxlen">The preferred maximum length, in bytes, of the returned data. If you specify MAX_PREFERRED_LENGTH, the NetUserEnum function allocates the amount of memory required for the data. If you specify another value in this parameter, it can restrict the number of bytes that the function returns. If the buffer size is insufficient to hold all entries, the function returns ERROR_MORE_DATA.</param>
        /// <param name="entriesread">A pointer to a value that receives the count of elements actually enumerated.</param>
        /// <param name="totalentries">A pointer to a value that receives the total number of entries that could have been enumerated from the current resume position. Note that applications should consider this value only as a hint.</param>
        /// <param name="resumeHandle">A pointer to a value that contains a resume handle which is used to continue an existing user search. The handle should be zero on the first call and left unchanged for subsequent calls. If this parameter is NULL, then no resume handle is stored.</param>
        /// <returns><see cref="NetApiStatus"/>.</returns>
        [DllImport("NetAPI32.dll", CharSet = CharSet.Unicode)]
        internal extern static NetApiStatus NetUserEnum(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            int level,
            NetApiFilter filter,
            ref IntPtr bufptr,
            int prefmaxlen,
            out int entriesread,
            out int totalentries,
            IntPtr resumeHandle);

        #endregion

        #region WTS Api

        /// <summary>
        /// Retrieves the session identifier of the console session. The console session is the session that is currently attached to the physical console. Note that it is not necessary that Remote Desktop Services be running for this function to succeed.
        /// </summary>
        /// <returns>The session identifier of the session that is attached to the physical console. If there is no session attached to the physical console, (for example, if the physical console session is in the process of being attached or detached), this function returns 0xFFFFFFFF.</returns>
        [DllImport("kernel32.dll")]
        public static extern uint WTSGetActiveConsoleSessionId();

        /// <summary>
        /// Obtains the primary access token of the logged-on user specified by the session ID. To call this function successfully, the calling application must be running within the context of the LocalSystem account and have the SE_TCB_NAME privilege.
        /// </summary>
        /// <param name="SessionId">A Remote Desktop Services session identifier. Any program running in the context of a service will have a session identifier of zero (0). You can use the WTSEnumerateSessions function to retrieve the identifiers of all sessions on a specified RD Session Host server. To be able to query information for another user's session, you need to have the Query Information permission. For more information, see Remote Desktop Services Permissions. To modify permissions on a session, use the Remote Desktop Services Configuration administrative tool.</param>
        /// <param name="phToken">If the function succeeds, receives a pointer to the token handle for the logged-on user. Note that you must call the CloseHandle function to close this handle.</param>
        /// <returns>If the function succeeds, the return value is a nonzero value.</returns>
        [DllImport("Wtsapi32.dll")]
        public static extern uint WTSQueryUserToken(uint SessionId, ref IntPtr phToken);

        /// <summary>
        /// Retrieves a list of sessions on a Remote Desktop Session Host (RD Session Host) server.
        /// </summary>
        /// <param name="hServer">A handle to the RD Session Host server.</param>
        /// <param name="Reserved">This parameter is reserved. It must be zero.</param>
        /// <param name="Version">The version of the enumeration request. This parameter must be 1.</param>
        /// <param name="ppSessionInfo">A pointer to an array of WTS_SESSION_INFO structures that represent the retrieved sessions. To free the returned buffer, call the WTSFreeMemory function.</param>
        /// <param name="pCount">A pointer to the number of WTS_SESSION_INFO structures returned in the ppSessionInfo parameter.</param>
        /// <returns>Returns zero if this function fails. If this function succeeds, a nonzero value is returned.</returns>
        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern int WTSEnumerateSessions(
            IntPtr hServer,
            int Reserved,
            int Version,
            ref IntPtr ppSessionInfo,
            ref int pCount);

        #endregion

        #region Shell API

        /// <summary>
        /// Copies the users account picture to a temporary directory and returns the path or returns various paths relating to user pictures.
        /// </summary>
        /// <param name="name">The name of a user account on this computer, or desired file name of the current users picture. Can be NULL to indicate the current users' name. Must be Microsoft Account Name for Microsoft Accounts.</param>
        /// <param name="flags">Options, see <see cref="GetUserPictureFlags"/>.</param>
        /// <param name="path">A pointer to a buffer that receives the path of the copied file. Cannot be NULL.</param>
        /// <param name="pathLength">Length of the buffer in chars.</param>
        [DllImport("shell32.dll", EntryPoint = "#261", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void SHGetUserPicturePath(string name, GetUserPictureFlags flags, StringBuilder path, int pathLength);

        /// <summary>
        /// Copies the users account picture to a temporary directory and returns the path or returns various paths relating to user pictures.
        /// </summary>
        /// <param name="name">The name of a user account on this computer, or desired file name of the current users picture. Can be NULL to indicate the current users' name. Must be Microsoft Account Name for Microsoft Accounts.</param>
        /// <param name="flags">Options, see <see cref="GetUserPictureFlags"/>.</param>
        /// <param name="desiredSrcExt">Desired filetype of the source picture. Defaults to .bmp if NULL is given.</param>
        /// <param name="path">A pointer to a buffer that receives the path of the copied file. Cannot be NULL.</param>
        /// <param name="pathLength">Length of the buffer in chars.</param>
        /// <param name="srcPath">Buffer to which the original path of the users picture is copied.</param>
        /// <param name="srcLength">Length of the source path buffer in chars.</param>
        [DllImport("shell32.dll", EntryPoint = "#810", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void SHGetUserPicturePathEx(string name, GetUserPictureFlags flags, string desiredSrcExt, StringBuilder path, int pathLength, StringBuilder srcPath, int srcLength);

        #endregion

        #region ADV API

        /// <summary>
        /// Creates a new access token that duplicates an existing token. This function can create either a primary token or an impersonation token.
        /// </summary>
        /// <param name="ExistingTokenHandle">A handle to an access token opened with TOKEN_DUPLICATE access.</param>
        /// <param name="dwDesiredAccess">Specifies the requested access rights for the new token. The DuplicateTokenEx function compares the requested access rights with the existing token's discretionary access control list (DACL) to determine which rights are granted or denied. To request the same access rights as the existing token, specify zero. To request all access rights that are valid for the caller, specify MAXIMUM_ALLOWED. For a list of access rights for access tokens, see Access Rights for Access-Token Objects.</param>
        /// <param name="lpThreadAttributes">A pointer to a SECURITY_ATTRIBUTES structure that specifies a security descriptor for the new token and determines whether child processes can inherit the token. If lpTokenAttributes is NULL, the token gets a default security descriptor and the handle cannot be inherited. If the security descriptor contains a system access control list (SACL), the token gets ACCESS_SYSTEM_SECURITY access right, even if it was not requested in dwDesiredAccess. To set the owner in the security descriptor for the new token, the caller's process token must have the SE_RESTORE_NAME privilege set.</param>
        /// <param name="TokenType">Specifies one of the following values from the TOKEN_TYPE enumeration.</param>
        /// <param name="ImpersonationLevel">Specifies a value from the <see cref="SecurityImpersonationLevel"/> enumeration that indicates the impersonation level of the new token.</param>
        /// <param name="DuplicateTokenHandle">A pointer to a variable that receives a handle to the duplicate token. This handle has TOKEN_IMPERSONATE and TOKEN_QUERY access to the new token. When you have finished using the new token, call the CloseHandle function to close the token handle.</param>
        /// <returns>True if successfull.</returns>
        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        internal static extern bool DuplicateTokenEx(
            IntPtr ExistingTokenHandle,
            uint dwDesiredAccess,
            IntPtr lpThreadAttributes,
            int TokenType,
            int ImpersonationLevel,
            ref IntPtr DuplicateTokenHandle);

        /// <summary>
        /// The OpenProcessToken function opens the access token associated with a process.
        /// </summary>
        /// <param name="ProcessHandle">A handle to the process whose access token is opened. The process must have the PROCESS_QUERY_INFORMATION access permission.</param>
        /// <param name="DesiredAccess">Specifies an access mask that specifies the requested types of access to the access token. These requested access types are compared with the discretionary access control list (DACL) of the token to determine which accesses are granted or denied.</param>
        /// <param name="TokenHandle">A pointer to a handle that identifies the newly opened access token when the function returns.</param>
        /// <returns>True if successfull.</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle, TokenAccessLevels DesiredAccess, out IntPtr TokenHandle);

        /// <summary>
        /// The GetTokenInformation function retrieves a specified type of information about an access token. The calling process must have appropriate access rights to obtain the information.
        /// </summary>
        /// <param name="TokenHandle">A handle to an access token from which information is retrieved. If TokenInformationClass specifies TokenSource, the handle must have TOKEN_QUERY_SOURCE access. For all other TokenInformationClass values, the handle must have TOKEN_QUERY access.</param>
        /// <param name="TokenInformationClass">Specifies a value from the TOKEN_INFORMATION_CLASS enumerated type to identify the type of information the function retrieves. Any callers who check the TokenIsAppContainer and have it return 0 should also verify that the caller token is not an identify level impersonation token. If the current token is not an app container but is an identity level token, you should return AccessDenied.</param>
        /// <param name="TokenInformation">A pointer to a buffer the function fills with the requested information. The structure put into this buffer depends upon the type of information specified by the TokenInformationClass parameter.</param>
        /// <param name="TokenInformationLength">Specifies the size, in bytes, of the buffer pointed to by the TokenInformation parameter. If TokenInformation is NULL, this parameter must be zero.</param>
        /// <param name="ReturnLength">A pointer to a variable that receives the number of bytes needed for the buffer pointed to by the TokenInformation parameter. If this value is larger than the value specified in the TokenInformationLength parameter, the function fails and stores no data in the buffer.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool GetTokenInformation(
            IntPtr TokenHandle,
            TokenInformationClass TokenInformationClass,
            IntPtr TokenInformation,
            uint TokenInformationLength,
            out uint ReturnLength);

        #endregion

        #region Kernel32 API

        /// <summary>
        /// Closes an open object handle.
        /// </summary>
        /// <param name="hObject">A valid handle to an open object.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// Retrieves the amount of RAM that is physically installed on the computer.
        /// </summary>
        /// <param name="totalMemoryInKilobytes">A pointer to a variable that receives the amount of physically installed RAM, in kilobytes.</param>
        /// <returns>If the function succeeds, it returns TRUE and sets the TotalMemoryInKilobytes parameter to a nonzero value.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetPhysicallyInstalledSystemMemory(out IntPtr totalMemoryInKilobytes);

        #endregion

    }

}