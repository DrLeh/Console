using System;
using System.Linq;

namespace DLeh.Console.Commands
{
    [Command("change-env", "changes environment to the number provided")]
    public class ChangeEnvironmentCommand : ICommand
    {
        public void Execute(ITerminalContext context)
        {
            if (context.Args.Any())
            {
                EnvironmentBase.SetEnvironment(int.Parse(context.Args.First()));
                return;
            }

            var environments = EnvironmentBase.EnvDict;
            if (environments.Count == 1)
            {
                EnvironmentBase.SetEnvironment(environments.Keys.First());
                return;
            }

            while (true)
            {
                Terminal.PrintLineColor(ConsoleColor.DarkCyan, "Please select an environment:");
                foreach (var env in environments)
                {
                    Terminal.PrintColor(ConsoleColor.White, "{0}) ", env.Key);
                    Terminal.PrintLineColor(ConsoleColor.Cyan, env.Value);
                }
                System.Console.Write("> ");
                var envIndex = System.Console.ReadLine();
                if (int.TryParse(envIndex, out var envNum) && EnvironmentBase.SetEnvironment(envNum))
                    break;
            }
        }
    }
}
