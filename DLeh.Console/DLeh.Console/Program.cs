﻿using System;
using C = System.Console;
using System.Linq;
using System.Collections.Generic;
using DLeh.Console.Commands;

namespace DLeh.Console
{
    internal static class DLehConsole
    {
        public static bool ShouldExit { get; set; }
        public static bool Headless { get; set; }

        public static bool CatchExceptions { get; set; } = true;
        public static bool PlaySound { get; set; }
    }

    public class Console<TContext>
        where TContext : ITerminalContext, new()
    {
        public void Start(IEnvironment e, string[] args)
        {
            EnvironmentBase.ConfigureEnvironment(e);

            if (args == null || args.Length == 0)
            {
                ExecuteCommand(new TContext { Args = new CommandArguments("change-env") });
                RunInteractively();
            }
            else if (args.Length == 1)
            {
                ExecuteCommand(new TContext { Args = new CommandArguments("change-env " + args[0]) });
                RunInteractively();
            }
            else
            {
                RunHeadless(args);
            }
        }

        private void RunHeadless(string[] arg)
        {
            DLehConsole.Headless = true;

            var env = arg[0];
            new ChangeEnvironmentCommand().Execute(new TContext { Args = env });

            var batches = new List<List<string>>();

            var b = new List<string>();
            foreach (var a in arg.Skip(1)) //skip env command
            {
                if (a == "&")
                {
                    batches.Add(b);
                    b = new List<string>();
                }
                else
                    b.Add(a);
            }
            batches.Add(b);

            foreach (var batch in batches)
            {
                var args = new CommandArguments(batch.ToArray());
                try
                {
                    Terminal.PutHistory(args.First());
                    ExecuteCommand(new TContext { Args = args }, true);
                    System.Environment.ExitCode = 0;
                }
                catch (Exception)
                {
                    System.Environment.ExitCode = 1;
                }
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Terminal.Good("Reverting to interactive since you're debugging");
                RunInteractively();
            }
        }

        public void RunInteractively()
        {
            Terminal.PrintLineColor(ConsoleColor.Green, "Enter command ('help' for help, 'exit' to quit):");

            while (true)
            {
                var envString = EnvironmentBase.Current.Name;
                C.Write($"{envString}> ");

                var s = Terminal.ReadWithAutocomplete();
                C.WriteLine();
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                try
                {
                    foreach (var ss in s.Split('\n'))
                    {
                        if (ExecuteCommand(new TContext() { Args = new CommandArguments(ss) }))
                            return;
                    }
                }
                catch (Exception e) when (DLehConsole.CatchExceptions)
                {
                    Terminal.Danger(e.Message);
                }
            }
        }

        private static bool ExecuteCommand(ITerminalContext context, bool echo = false)
        {
            var command = context.Args.Shift().ToLowerInvariant().Trim();
            var cmd = CommandList.GetCommandFromName<TContext>(command);
            if (cmd == null)
            {
                if (int.TryParse(command, out var a))
                {
                    Terminal.Info($"Changing environment to {a}");
                    new ChangeEnvironmentCommand().Execute(new TContext { Args = $"{a}" });
                }
                else
                    Terminal.Danger($"Unknown command '{command }'");
                return false;
            }

            if (echo)
                Terminal.Info(command + " " + context.Args.Original);

            try
            {
                if (cmd is ICommand cmda)
                    cmda.Execute(context);
                else if (cmd is ICommand<TContext> cmdb)
                    cmdb.Execute((TContext)context);
            }
            catch (Exception e) when (DLehConsole.CatchExceptions)
            {
                Terminal.Danger(e.Message);
                var inner = e.GetInnermostException();
                if (inner != e)
                    Terminal.Danger(inner.Message);

                if (DLehConsole.Headless)
                    throw;
            }

            return DLehConsole.ShouldExit;
        }

        [Command("exit", "Exits application")]
        [Command("quit", "Exits application")]
        public class ExitCommand : ICommand
        {
            public void Execute(ITerminalContext context)
            {
                DLehConsole.ShouldExit = true;
            }
        }

        [Command("catch", "Toggles whether exceptions will be caught in the program loop or if they will flow through to the debugger")]
        public class CatchCommand : ICommand
        {
            public void Execute(ITerminalContext context)
            {
                DLehConsole.CatchExceptions = !DLehConsole.CatchExceptions;
                if (DLehConsole.CatchExceptions)
                    Terminal.Good("Exception Catching ON");
                else
                    Terminal.Danger("Exception Catching OFF");
            }
        }

        [Command("cls", "Clears output")]
        [Command("clear", "Clears output")]
        public class ClearConsoleCommand : ICommand
        {
            public void Execute(ITerminalContext context)
            {
                C.Clear();
            }
        }
    }
}
