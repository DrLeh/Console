using System;

namespace DLeh.Console.Commands
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string CommandName { get; }
        public string? Description { get; }
        public string[]? Arguments { get; }

        public CommandAttribute(string command)
        {
            CommandName = command;
        }

        public CommandAttribute(string command, string description, params string[] argNames)
        {
            CommandName = command;
            Description = description;
            Arguments = argNames;
        }
    }
}
