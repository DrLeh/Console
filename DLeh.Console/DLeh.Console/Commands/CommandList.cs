using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DLeh.Console.Commands
{
    public static class CommandList
    {
        private static List<Type> _typeCache;

        public static IEnumerable<Type> GetCommandTypes()
        {
            if (_typeCache == null)
            {
                var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Concat(Assembly.GetEntryAssembly().GetTypes());

                _typeCache = types.Where(x => !x.IsInterface && typeof(ICommandBase).IsAssignableFrom(x))
                    .ToList();
            }
            return _typeCache;
        }

        public static IEnumerable<string> GetCommandNames()
        {
            return GetCommandTypes().SelectMany(x => x.GetCustomAttributes(typeof(CommandAttribute)).Cast<CommandAttribute>().Select(tca => tca.CommandName)).Where(x => x != null);
        }

        public static IEnumerable<CommandAttribute> GetCommandAttributes()
        {
            return GetCommandTypes().SelectMany(x => x.GetCustomAttributes(typeof(CommandAttribute)).Cast<CommandAttribute>()).Where(x => x != null);
        }

        public static ICommandBase? GetCommandFromName<TContext>(string commandName)
        {
            var type = GetCommandTypes().FirstOrDefault(x => x.GetCustomAttributes(typeof(CommandAttribute)).Cast<CommandAttribute>().Any(tca => tca.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
            if (type == null)
                return null;

            if (type.ContainsGenericParameters)
            {
                var gen = type.MakeGenericType(typeof(TContext));
                return (ICommandBase)Activator.CreateInstance(gen);
            }

            return (ICommandBase)Activator.CreateInstance(type);
        }

        public static string GetCommandDescription(string commandName)
        {
            var type = GetCommandTypes().FirstOrDefault(x => x.GetCustomAttributes(typeof(CommandAttribute)).Cast<CommandAttribute>().Any(tca => tca.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
            if (type == null)
                return "no description";

            return type.GetCustomAttributes(typeof(CommandAttribute)).Cast<CommandAttribute>().Select(tca => tca.Description).FirstOrDefault(d => !string.IsNullOrEmpty(d)) ?? "no description";
        }

        public static IEnumerable<string> GetCommandArguments(string commandName)
        {
            var type = GetCommandTypes().FirstOrDefault(x => x.GetCustomAttributes(typeof(CommandAttribute)).Cast<CommandAttribute>().Any(tca => tca.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase)));

            return (type?.GetCustomAttributes(typeof(CommandAttribute)).Cast<CommandAttribute>().SelectMany(tca => tca.Arguments)).OrEmptyIfNull();
        }

        public static IEnumerable<string> CommandSearch(string search)
        {
            return GetCommandTypes().SelectMany(x =>
                x.GetCustomAttributes(typeof(CommandAttribute)).Cast<CommandAttribute>())
                .Where(x => x.CommandName.StartsWith(search))
                .Select(x => x.CommandName)
                .OrderBy(x => x);
        }
    }
}
