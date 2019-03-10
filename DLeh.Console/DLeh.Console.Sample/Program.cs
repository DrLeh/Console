using DLeh.Console.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLeh.Console.Sample
{
    class Program
    {
        [STAThread] //necessary for forms clipboard
        static void Main(string[] args)
        {
            new Console<MyContext>()
                .Start(new MyEnvironment(), args)
                ;
        }
    }

    public class MyEnvironment : EnvironmentBase
    {
        private Dictionary<string, string> DbConnections = new Dictionary<string, string>
        {
            ["DB_LOCAL"] = "localhost",
            ["DB_DEV"] = "dev"
        };

        public string DbConnectionString => DbConnections[$"DB_{Name}"];
    }


    public class MyContext : ITerminalContext<MyEnvironment>
    {
        public string MyCustomConfigValue = "Cust_Val";

        public CommandArguments Args { get; set; } = CommandArguments.Empty;

        public MyEnvironment Environment { get; set; } = Console.EnvironmentBase.Current as MyEnvironment;
    }

    public interface IMyCommand : ICommand<MyContext>
    {
    }


    [Command("my-typed-cmd", "Description of what the command does")]
    public class MyTypeCommand : IMyCommand
    {
        public void Execute(MyContext context)
        {

        }
    }
}
