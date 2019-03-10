using System;
using System.Linq;

namespace DLeh.Console.Commands
{
    [Command("help", "list all commands or get details about a single command", "command name")]
    public class HelpCommand : ICommand
    {
        public void Execute(ITerminalContext context)
        {
            if (context.Args.Any())
            {
                string commandName = context.Args[0];
                Terminal.Info($"{commandName} command:");
                Terminal.Info($"  {CommandList.GetCommandDescription(commandName)}");

                var args = CommandList.GetCommandArguments(commandName);
                if (args.Any())
                    Terminal.Info("Arguments:");

                for(int i = 0; i < args.Count(); i++)
                foreach (var arg in args)
                {
                    Terminal.Info($"  [{i}]: {arg}");
                }
                return;
            }
            Terminal.Info("Available commands:");
            var commands = CommandList.GetCommandAttributes().OrderBy(x => x.CommandName).ToList();
            var maxLength = commands.Select(c => c.CommandName.Length).Max();
            var descWidth = System.Console.WindowWidth - maxLength - 3;
            foreach (var command in commands)
            {
                Terminal.PrintColor(ConsoleColor.Green, command.CommandName.PadRight(maxLength + 2));
                Terminal.PrintColor(ConsoleColor.Gray, TruncateElipses(command.Description ?? "", descWidth));
                Terminal.Line();
            }
        }

        public static string TruncateElipses(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            if (value.Length <= maxLength)
                return value;
            return value.Substring(0, maxLength - 3) + "...";
        }
    }
}

