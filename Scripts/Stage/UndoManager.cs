using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance;

    private Stack<Command> undoStack = new Stack<Command>();
    private Stack<Command> redoStack = new Stack<Command>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void RecordCommand(Command command)
    {
        if (command == null) return;

        undoStack.Push(command);
        redoStack.Clear();

    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            Command commandToUndo = undoStack.Pop();

            commandToUndo.Undo();

            redoStack.Push(commandToUndo);
        }
        else
        {
            Debug.Log("Undo 스택 비었습니다");
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            Command commandToRedo = redoStack.Pop();

            commandToRedo.Redo();

            undoStack.Push(commandToRedo);
        }
        else
        {
            Debug.Log("Redo 스택 비었습니다");
        }
    }
}