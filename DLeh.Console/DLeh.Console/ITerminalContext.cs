namespace DLeh.Console
{
    public interface ITerminalContext
    {
        CommandArguments Args { get; set; }
    }

    public interface ITerminalContext<TEnv> : ITerminalContext
        where TEnv : IEnvironment
    {
        TEnv Environment { get; set; }
    }
}
