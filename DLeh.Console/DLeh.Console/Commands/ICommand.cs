namespace DLeh.Console.Commands
{
    public interface ICommandBase
    {
    }

    public interface ICommand<in TContext> : ICommandBase
        where TContext : ITerminalContext
    {
        void Execute(TContext context);
    }

    public interface ICommand : ICommand<ITerminalContext>
    {
    }
}
