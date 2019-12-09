using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Woof.SystemEx {

    /// <summary>
    /// Represents system hosts file.
    /// </summary>
    public class HostsFile {

        /// <summary>
        /// Gets the IP pointed with the host name specified if exists. Null otherwise.
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public IPAddress this[string hostName] {
            get {
                if (Tokens == null) return null;
                var index = GetHostIndex(hostName) - 2;
                return index >= 0 ? IPAddress.Parse(Tokens[index].Value) : null;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the content was modified since loaded.
        /// </summary>
        public bool IsModified { get; private set; }

        /// <summary>
        /// Creates system hosts file logical representation.
        /// </summary>
        public HostsFile() => Tokens = new Lexer(File.ReadAllText(Target)).Tokens.ToList();

        /// <summary>
        /// Adds a new entry at the end.
        /// </summary>
        /// <param name="hostName">Host name.</param>
        /// <param name="ip">Host IP address.</param>
        /// <param name="comment">Optional comment (without '#').</param>
        public void Append(string hostName, IPAddress ip, string comment = null) {
            AppendStart();
            AppendIP(ip);
            AppendSpace();
            AppendHostName(hostName);
            if (comment != null) {
                AppendSpace();
                AppendComment(comment);
            }
        }

        /// <summary>
        /// Adds a comment line at the end.
        /// </summary>
        /// <param name="text">Comment text (without '#').</param>
        public void Comment(string text) {
            AppendStart();
            AppendComment(text);
        }

        /// <summary>
        /// Tests if specified host entry exists.
        /// </summary>
        /// <param name="hostName">Host name.</param>
        /// <returns>True if matching entry exists.</returns>
        public bool Exists(string hostName) => Tokens?.Any(i => i.Type == Lexer.LexemeType.HostName && i.Value == hostName) ?? false;

        /// <summary>
        /// Appends a new line at the end.
        /// </summary>
        public void NewLine() {
            AppendStart();
            Tokens.Add(new Lexer.Lexeme { Type = Lexer.LexemeType.Whitespace, Value = Environment.NewLine });
        }

        /// <summary>
        /// Removes a host entry.
        /// </summary>
        /// <param name="hostName">Host name.</param>
        public void Remove(string hostName) {
            var index = GetHostIndex(hostName);
            if (index < 2) return;
            var first = index - 2;
            var next = GetNextEntryIndex(index);
            if (next > 0) {
                var tokens = new List<Lexer.Lexeme>();
                for (int i = 0, n = Tokens.Count; i < n; i++) {
                    if (i < first || i >= next) tokens.Add(Tokens[i]);
                }
                IsModified = true;
                Tokens = tokens.ToList();
                
            }
            else {
                IsModified = true;
                Tokens = Tokens.Take(first - 1).ToList();
            }
        }

        /// <summary>
        /// Returns current hosts content as string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => String.Join("", Tokens.Select(i => i.Value));

        /// <summary>
        /// Writes current hosts content to string.
        /// </summary>
        public void Write() => File.WriteAllText(Target, ToString());

        #region Private methods

        private void AppendStart() {
            IsModified = true;
            if (Tokens.Last().Type != Lexer.LexemeType.Whitespace) Tokens.Add(new Lexer.Lexeme { Type = Lexer.LexemeType.Whitespace, Value = Environment.NewLine });
        }

        private void AppendIP(IPAddress ip) => Tokens.Add(new Lexer.Lexeme { Type = Lexer.LexemeType.IP, Value = ip.ToString() });

        private void AppendHostName(string hostName) => Tokens.Add(new Lexer.Lexeme { Type = Lexer.LexemeType.HostName, Value = hostName.Trim() });

        private void AppendSpace() => Tokens.Add(new Lexer.Lexeme { Type = Lexer.LexemeType.Whitespace, Value = "\t" });


        private void AppendComment(string comment) => Tokens.Add(new Lexer.Lexeme { Type = Lexer.LexemeType.Comment, Value = $"# {comment}" });

        private int GetHostIndex(string hostName) => Tokens.IndexOf(new Lexer.Lexeme { Type = Lexer.LexemeType.HostName, Value = hostName });

        private int GetNextEntryIndex(int offset) {
            for (int i = offset + 1, n = Tokens.Count; i < n; i++) if (Tokens[i].Type == Lexer.LexemeType.IP) return i;
            return -1;
        }

        #endregion

        #region Private data

        /// <summary>
        /// Gets the target file path.
        /// </summary>
        private static string Target => Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");

        /// <summary>
        /// Current token list.
        /// </summary>
        private List<Lexer.Lexeme> Tokens;

        #endregion

        #region Classes

        /// <summary>
        /// Host entry.
        /// </summary>
        public struct Host {
            /// <summary>
            /// The name attribute.
            /// </summary>
            public string HostName;
            /// <summary>
            /// IP address attribute.
            /// </summary>
            public IPAddress IP;
        }

        class Lexer {

            private readonly string Text;

            public Lexer(string text) => Text = text;
            public IEnumerable<Lexeme> Tokens {
                get {
                    int i = 0, p = 0, n = Text.Length;
                    int getOffset() => p - (p > 0 && Text[p - 1] == '\r' ? 1 : 0);
                    int getLength() => i - p + (i == n - 1 ? 1 : 0) - (Text[i - 1] == '\r' ? 1 : 0) + (p > 0 && Text[p - 1] == '\r' ? 1 : 0);
                    var state = LexemeType.Whitespace;
                    var gotIP = false;
                    for (; i < n; i++) {
                        if (Text[i] == ' ' || Text[i] == '\t' || Text[i] == '\r' || Text[i] == '\n' || i == n - 1) {
                            switch (state) {
                                case LexemeType.IP:
                                    yield return new Lexeme { Type = LexemeType.IP, Value = Text.Substring(getOffset(), getLength()) };
                                    state = LexemeType.Whitespace;
                                    p = i;
                                    gotIP = true;
                                    break;
                                case LexemeType.HostName:
                                    yield return new Lexeme { Type = LexemeType.HostName, Value = Text.Substring(getOffset(), getLength()) };
                                    state = LexemeType.Whitespace;
                                    p = i;
                                    gotIP = false;
                                    break;
                                case LexemeType.Comment:
                                    if (Text[i] == '\n') {
                                        yield return new Lexeme { Type = LexemeType.Comment, Value = Text.Substring(getOffset(), getLength()) };
                                        state = LexemeType.Whitespace;
                                        p = i;
                                    }
                                    break;
                            }
                        }
                        else {
                            if (state == LexemeType.Whitespace) {
                                if (p > 0) yield return new Lexeme { Type = LexemeType.Whitespace, Value = Text.Substring(getOffset(), getLength()) };
                                if (Text[i] == '#') {
                                    state = LexemeType.Comment;
                                    p = i;
                                }
                                else if (!gotIP) {
                                    state = LexemeType.IP;
                                    p = i;
                                }
                                else {
                                    state = LexemeType.HostName;
                                    p = i;
                                }
                            }
                        }
                    }
                }
            }

            public struct Lexeme {

                public LexemeType Type;
                public string Value;
                public override string ToString() => $"[{Type}] = \"{Value.Replace('\r', '←').Replace('\n', '↓').Replace(' ', '·').Replace('\t', '→')}\"";

            }

            public enum LexemeType { Whitespace, IP, HostName, Comment }

        }

        #endregion

    }

}