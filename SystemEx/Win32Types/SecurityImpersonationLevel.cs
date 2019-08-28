﻿namespace Woof.SystemEx.Win32Types {

    /// <summary>
    /// Contains values that specify security impersonation levels. Security impersonation levels govern the degree to which a server process can act on behalf of a client process.
    /// </summary>
    internal enum SecurityImpersonationLevel {
        
        /// <summary>
        /// The server process cannot obtain identification information about the client, and it cannot impersonate the client. It is defined with no value given, and thus, by ANSI C rules, defaults to a value of zero.
        /// </summary>
        SecurityAnonymous = 0,
        
        /// <summary>
        /// The server process can obtain information about the client, such as security identifiers and privileges, but it cannot impersonate the client. This is useful for servers that export their own objects, for example, database products that export tables and views. Using the retrieved client-security information, the server can make access-validation decisions without being able to use other services that are using the client's security context.
        /// </summary>
        SecurityIdentification = 1,
        
        /// <summary>
        /// The server process can impersonate the client's security context on its local system. The server cannot impersonate the client on remote systems.
        /// </summary>
        SecurityImpersonation = 2,
        
        /// <summary>
        /// The server process can impersonate the client's security context on remote systems.
        /// </summary>
        SecurityDelegation = 3

    }

}