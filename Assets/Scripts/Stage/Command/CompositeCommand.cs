using System.Collections.Generic;
using System.Linq; 

public class CompositeCommand : Command
{
    private List<Command> commands;

    public CompositeCommand(List<Command> commandList)
    {
        commands = new List<Command>(commandList);
    }

    public override void Execute()
    {
        foreach (var command in commands)
        {
            command.Execute();
        }
    }

    public override void Undo()
    {
        foreach (var command in Enumerable.Reverse(commands))
        {
            command.Undo();
        }
    }

    public override void Redo()
    {

        foreach (var command in commands)
        {
            command.Redo();
        }
    }
}