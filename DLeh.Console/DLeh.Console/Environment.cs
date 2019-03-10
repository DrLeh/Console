using System;
using System.Collections.Generic;
using System.Linq;

namespace DLeh.Console
{
    public interface IEnvironment
    {
        string? Name { get; set; }
        IEnumerable<string> Environments { get; }
    }

    public class EnvironmentBase : IEnvironment
    {
        public virtual IEnumerable<string> Environments => new[]
        {
            "LOCAL",
        };

        public string? Name { get; set; }

        public static IEnvironment Current { get; private set; }

        public static bool SetEnvironment(int number)
        {
            if (!EnvDict.TryGetValue(number, out string? _))
            {
                Terminal.Danger($"No such environment {number}");
                return false;
            }
            Current.Name = EnvDict[number];
            
            return true;
        }

        public static Dictionary<int, string?> EnvDict { get; private set; } = new Dictionary<int, string?>();

        public static void ConfigureEnvironment<TEnv>(TEnv env)
            where TEnv : IEnvironment
        {
            EnvDict = env.Environments.Select((item, index) => new { Index = index + 1, Item = item }).ToDictionary(x => x.Index, x => x.Item);
            Current = env;
        }
    }
}
