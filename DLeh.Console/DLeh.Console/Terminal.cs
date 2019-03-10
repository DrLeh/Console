using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using C = System.Console;
using DLeh.Console.Commands;
using DLeh.Console.Clipboard;

namespace DLeh.Console
{
    /// <summary>
    /// Helper class to more easily access frequently-used user IO
    /// </summary>
    public static class Terminal
    {
        private static readonly IClipboardManager _clipboard;

        static Terminal()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes());

            var t = types.FirstOrDefault(x => !x.IsInterface && typeof(IClipboardManager).IsAssignableFrom(x));
            if (t != null)
                _clipboard = (IClipboardManager)Activator.CreateInstance(t);
        }

        public static void Good(string fmt, params object[] args)
        {
            Green(fmt, args);
        }
        public static void Danger(string fmt, params object[] args)
        {
            Red(fmt, args);
        }
        public static void Info(string? fmt = null, params object[] args)
        {
            White(fmt, args);
        }
        public static void Warn(string fmt, params object[] args)
        {
            Yellow(fmt, args);
        }

        public static void Green(string fmt, params object[] args)
        {
            PrintLineColor(ConsoleColor.Green, fmt, args);
        }
        public static void Red(string fmt, params object[] args)
        {
            PrintLineColor(ConsoleColor.Red, fmt, args);
        }
        public static void White(string? fmt = null, params object[] args)
        {
            PrintLineColor(ConsoleColor.White, fmt, args);
        }


        public static void Blue(string fmt, params object[] args)
        {
            PrintLineColor(ConsoleColor.Blue, fmt, args);
        }
        public static void Cyan(string fmt, params object[] args)
        {
            PrintLineColor(ConsoleColor.DarkCyan, fmt, args);
        }
        public static void Yellow(string fmt, params object[] args)
        {
            PrintLineColor(ConsoleColor.DarkYellow, fmt, args);
        }
        public static void Magenta(string fmt, params object[] args)
        {
            PrintLineColor(ConsoleColor.Magenta, fmt, args);
        }
        public static void Line()
        {
            C.WriteLine();
        }

        public static void GoodBad(string good, string bad, bool result)
        {
            if (result)
                Good(good);
            else
                Danger(bad);
        }

        public static object locker = new object();
        public static void PrintColor(ConsoleColor color, string? fmt, params object[] args)
        {
            lock (locker)
            {
                ConsoleColor currentColor = C.ForegroundColor;
                C.ForegroundColor = color;
                if (args.OrEmptyIfNull().Any())
                    C.Write(fmt, args);
                else
                    C.Write(fmt);
                C.ForegroundColor = currentColor;
            }
        }

        public static void PrintLineColor(ConsoleColor color, string? fmt, params object[] args)
        {
            PrintColor(color, fmt, args);
            C.WriteLine();
        }

        //overload without args needed for printing strings with {}s in them.
        public static void PrintColor(ConsoleColor color, string message)
        {
            lock (locker)
            {
                ConsoleColor currentColor = C.ForegroundColor;
                C.ForegroundColor = color;
                C.Write(message);
                C.ForegroundColor = currentColor;
            }
        }

        public static string ReadUntilProvided(string prompt)
        {
            string res = "";
            while (string.IsNullOrWhiteSpace(res))
            {
                Yellow(prompt);
                res = C.ReadLine();
            }
            return res;
        }
        public static string ReadUntilProvidedProtected(string prompt)
        {
            string res = "";
            while (string.IsNullOrWhiteSpace(res))
            {
                Yellow(prompt);
                ConsoleKeyInfo key;
                do
                {
                    key = C.ReadKey(true);

                    // Backspace Should Not Work
                    if (key.Key != ConsoleKey.Backspace)
                    {
                        res += key.KeyChar;
                        C.Write("*");
                    }
                    else
                    {
                        C.Write("\b");
                    }
                }
                // Stops Receving Keys Once Enter is Pressed
                while (key.Key != ConsoleKey.Enter);
            }
            C.WriteLine();
            return res.Trim();
        }

        //for printing things as json objects. Makes it easy to see what an object contains when debugging
        public static class Json
        {
            static string Serialize(object obj) => JsonConvert.SerializeObject(obj);

            public static void PrintColor(ConsoleColor color, object obj)
            {
                var serialized = Serialize(obj);
                Terminal.PrintColor(color, serialized);
            }

            public static void PrintLineColor(ConsoleColor color, object obj)
            {
                PrintColor(color, obj);
                C.WriteLine();
            }

            public static void Green(object obj)
            {
                PrintLineColor(ConsoleColor.Green, obj);
            }
            public static void Red(object obj)
            {
                PrintLineColor(ConsoleColor.Red, obj);
            }
            public static void Blue(object obj)
            {
                PrintLineColor(ConsoleColor.Blue, obj);
            }
            public static void Cyan(object obj)
            {
                PrintLineColor(ConsoleColor.DarkCyan, obj);
            }
            public static void White(object obj)
            {
                PrintLineColor(ConsoleColor.White, obj);
            }
        }

        static readonly List<string> _history = new List<string>();
        static int _histIndex = 0;

        public static string? ReadWithAutocomplete()
        {
            var sb = new StringBuilder();
            var originalPosition = new { left = C.CursorLeft, top = C.CursorTop };
            while (true)
            {
                int searchIndex = 0;

                var key = C.ReadKey(true);

            HandleKey:

                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    var search = sb.ToString();
                    var pos = new { left = C.CursorLeft - search.Length, top = C.CursorTop };

                TabLoop:

                    var searchTrimmed = search.Replace("\0", "").Trim();
                    var results = CommandList.CommandSearch(searchTrimmed).ToList();
                    if (!results.Any())
                        continue;

                    if (searchIndex > results.Count - 1)
                        searchIndex = 0;
                    var result = results[searchIndex];

                    if (!string.IsNullOrEmpty(result) && result != sb.ToString())
                    {
                        Blank(originalPosition.left, originalPosition.top);
                        C.SetCursorPosition(pos.left, pos.top);
                        sb = new StringBuilder(result);
                        C.Write(result);

                        key = C.ReadKey(true);
                        if (key.Key == ConsoleKey.Tab)
                        {
                            searchIndex++;
                            goto TabLoop; //sorry dijkstra
                        }

                        goto HandleKey;
                    }
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    var str = sb.ToString();
                    if (!string.IsNullOrWhiteSpace(str))
                        sb = new StringBuilder(str.Substring(0, str.Length - 1));
                    searchIndex = 0;

                    if (C.CursorLeft > originalPosition.left)
                    {
                        C.SetCursorPosition(C.CursorLeft - 1, C.CursorTop);
                        C.Write(" ");
                        C.SetCursorPosition(C.CursorLeft - 1, C.CursorTop);
                    }
                    _histIndex = _history.Count;
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    return null;
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    _histIndex--;
                    if (_histIndex < 0)
                        _histIndex = 0;
                    if (_history.Any())
                    {
                        Blank(originalPosition.left, originalPosition.top);

                        C.SetCursorPosition(originalPosition.left, originalPosition.top);
                        var text = _history[_histIndex];
                        sb = new StringBuilder(text);
                        C.Write(text);
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    _histIndex++;
                    if (_histIndex > _history.Count - 1)
                        _histIndex = _history.Count - 1;
                    if (_history.Any())
                    {
                        Blank(originalPosition.left, originalPosition.top);

                        C.SetCursorPosition(originalPosition.left, originalPosition.top);
                        var text = _history[_histIndex];
                        sb = new StringBuilder(text);
                        C.Write(text);
                    }
                }
                else if (key.Key == ConsoleKey.V && key.Modifiers == ConsoleModifiers.Control)
                {
                    if (_clipboard != null)
                    {
                        var text = _clipboard.Paste();
                        sb.Append(text);
                        C.Write(text);
                    }
                }
                else if (key.Key == ConsoleKey.X && key.Modifiers == ConsoleModifiers.Control)
                {
                    Blank(originalPosition.left, originalPosition.top);
                    sb.Clear();
                    C.SetCursorPosition(originalPosition.left, originalPosition.top);
                }
                else if (key.Key == ConsoleKey.C && key.Modifiers == ConsoleModifiers.Alt)
                {
                    _clipboard?.Copy(sb.ToString());
                }
                else
                {
                    sb.Append(key.KeyChar);
                    C.Write(key.KeyChar);
                    searchIndex = 0;
                }
            }
            var ret = sb.ToString();
            _history.Add(ret);
            _histIndex = _history.Count();
            if (_histIndex < 0)
                _histIndex = 0;
            return ret;
        }

        public static void PutHistory(string value)
        {
            _history.Add(value);
            _histIndex++;
        }

        private static void Blank(int left, int top)
        {
            var lengthToOverwrite = C.CursorLeft - left;
            C.SetCursorPosition(left, top);
            C.Write(string.Join("", Enumerable.Repeat(" ", lengthToOverwrite)));
        }

        public static void SetTitle(string environment)
        {
            var sb = new StringBuilder();
            sb.Append("DLeh Console | Environment: ").Append(environment);

            C.Title = sb.ToString();
        }

        public static void CopyToClipboard(string clip)
        {
            _clipboard?.Copy(clip);
        }
    }
}