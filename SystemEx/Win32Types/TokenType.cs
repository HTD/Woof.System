namespace Woof.SystemEx.Win32Types {

    /// <summary>
    /// Contains values that differentiate between a primary token and an impersonation token.
    /// </summary>
    internal enum TokenType {
        
        /// <summary>
        /// Indicates a primary token.
        /// </summary>
        TokenPrimary = 1,

        /// <summary>
        /// Indicates an impersonation token.
        /// </summary>
        TokenImpersonation = 2

    }

}