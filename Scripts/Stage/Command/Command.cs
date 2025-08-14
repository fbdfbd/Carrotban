
using UnityEngine; 

public abstract class Command
{
    public abstract void Execute();
    public abstract void Undo();
    public virtual void Redo()
    {
        Execute();
    }
}