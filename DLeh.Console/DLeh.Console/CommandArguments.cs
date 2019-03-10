using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DLeh.Console
{
    public sealed class CommandArguments : IEnumerable<string>
    {
        public static CommandArguments Empty = new CommandArguments(string.Empty);

        private readonly List<string> _positionals;
        private readonly Dictionary<string, string> _named;
        public readonly string Original;

        public static implicit operator CommandArguments(string s) => new CommandArguments(s);

        public CommandArguments(string commandLine)
        {
            var trimmed = commandLine.Trim().Trim('\0');
            Original = trimmed;
            _positionals = new List<string>();
            _named = new Dictionary<string, string>();

            var parts = Preparse(trimmed);
            Parse(parts);
        }

        public static string[] Preparse(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
                return new string[0];

            var parts = new List<string>();
            var i = 0;
            while (i < commandLine.Length)
            {
                var c = commandLine[i];
                if (c == '"' || c == '\'')
                {
                    List<char> quotedChars = new List<char>();
                    char quote = c;
                    i = ParseQuoted(commandLine, ++i, quotedChars, quote);
                    parts.Add(new string(quotedChars.ToArray()));
                    i++;
                    continue;
                }
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }
                else
                {
                    var quotedChars = new List<char>();
                    while (true)
                    {
                        if (i >= commandLine.Length)
                        {
                            parts.Add(new string(quotedChars.ToArray()));
                            break;
                        }
                        char d = commandLine[i];
                        if (d == '"' || d == '\'')
                        {
                            char quote = d;
                            i = ParseQuoted(commandLine, ++i, quotedChars, quote);
                            i++;
                            continue;
                        }
                        if (char.IsWhiteSpace(d))
                        {
                            parts.Add(new string(quotedChars.ToArray()));
                            break;
                        }
                        quotedChars.Add(d);
                        i++;
                    }
                    i++;
                    continue;
                }
            }
            return parts.ToArray();
        }

        private static int ParseQuoted(string commandLine, int i, List<char> quotedChars, char quote)
        {
            while (true)
            {
                if (i >= commandLine.Length)
                    throw new Exception("Cannot find end quote");
                var d = commandLine[i];
                if (d == '\\')
                {
                    i++;
                    if (i >= commandLine.Length)
                        throw new Exception("Cannot find end quote");
                    quotedChars.Add(commandLine[i]);
                    i++;
                    continue;
                }

                if (d == quote)
                    break;
                quotedChars.Add(d);
                i++;
            }
            return i;
        }

        public CommandArguments(string[] args)
        {
            _positionals = new List<string>();
            _named = new Dictionary<string, string>();
            Parse(args);
        }

        public string Shift()
        {
            if (_positionals == null || _positionals.Count == 0)
                return null;
            var s = _positionals[0];
            _positionals.RemoveAt(0);
            return s;
        }

        public int Length => _positionals.Count;

        private void Parse(IEnumerable<string> parts)
        {
            var donePositionals = false;

            //ignore stuff after &
            var partsBeforeAmp = new List<string>();
            foreach (var p in parts)
            {
                if (p == "&")
                    break;
                else
                    partsBeforeAmp.Add(p);
            }

            foreach (var part in partsBeforeAmp)
            {
                if (part.Contains("="))
                {
                    donePositionals = true;
                    var kvp = part.Split(new char[] { '=' }, 2);
                    var value = ProcessValue(kvp[1]);
                    _named.Add(kvp[0].ToLowerInvariant(), value);
                    continue;
                }

                if (donePositionals)
                    throw new Exception("All positional arguments must come before all named arguments");

                _positionals.Add(ProcessValue(part));
            }
        }

        private static string ProcessValue(string value)
        {
            // TODO: Do we need to handle escape sequences?
            if (value.Length > 1 && value.StartsWith("@"))
                return File.ReadAllText(value.Substring(1));
            return value;
        }

        public string this[int idx] => Get(idx);

        public string this[string name] => Get(name);

        public string Get(int idx, string defaultValue = null)
        {
            var value = defaultValue;
            if (idx < _positionals.Count && !string.IsNullOrWhiteSpace(_positionals[idx]))
                value = _positionals[idx];

            if (value == null)
                throw new Exception("Expected argument at position " + idx + " but only have " + _positionals.Count + " arguments");
            return value;
        }

        public T Get<T>(int idx, Func<string, T> convert)
        {
            var s = Get(idx);
            return convert(s);
        }

        public T Get<T>(int idx)
        {
            return (T)Convert.ChangeType(Get(idx), typeof(T));
        }

        public T Get<T>(string name)
        {
            return (T)Convert.ChangeType(Get(name), typeof(T));
        }

        public T GetOrDefault<T>(int idx, T defaultValue = default(T))
        {
            if (idx > Length)
                return defaultValue;
            try
            {
                var def = string.Empty;
                if (typeof(T) == typeof(string))
                    def = (string)Convert.ChangeType(defaultValue, typeof(string));

                return (T)Convert.ChangeType(Get(idx, def), typeof(T));
            }
            catch (Exception) { }
            return defaultValue;
        }

        public T GetOrDefault<T>(string name, T defaultValue = default(T))
        {
            try
            {
                return (T)Convert.ChangeType(Get(name), typeof(T));
            }
            catch (Exception) { }
            return defaultValue;
        }

        public string GetAfter(int startPos)
        {
            return string.Join(" ", _positionals.Skip(startPos));
        }

        public string Get(string name, string defaultValue = null)
        {
            var value = defaultValue;
            var key = name.ToLowerInvariant();
            if (_named.ContainsKey(key))
                value = _named[key];

            if (value == null)
                throw new Exception("Expected argument with name " + name + " but it was not provided.");
            return value;
        }

        public string Consume(string name, string defaultValue = null)
        {
            var value = Get(name, defaultValue);
            if (_named.ContainsKey(name.ToLowerInvariant()))
                _named.Remove(name.ToLowerInvariant());
            return value;
        }

        public IReadOnlyDictionary<string, string> Named => _named;

        public IReadOnlyList<string> Positionals => _positionals;

        public T Get<T>(string name, Func<string, T> convert)
        {
            var s = Get(name);
            return convert(s);
        }

        public IEnumerable<T> Enumerate<T>(int skip = 0)
        {
            return this.Skip(skip).Select(x => (T)Convert.ChangeType(x, typeof(T)));
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _positionals.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
