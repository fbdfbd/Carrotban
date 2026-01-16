using UnityEngine;
using UnityEditor;

public class MissingScriptCleaner : EditorWindow
{
    [MenuItem("Tools/Cleanup/Missing Scripts In Scene")]
    private static void CleanupMissingScriptsInScene()
    {
        int goCount = 0;
        int compCount = 0;
        int removedCount = 0;

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        foreach (GameObject go in allObjects)
        {
            goCount++;
            Component[] components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Undo.RegisterCompleteObjectUndo(go, "Remove missing script");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    removedCount++;
                }
                else
                {
                    compCount++;
                }
            }
        }

        Debug.Log($"[MissingScriptCleaner] GameObjects: {goCount}, Components: {compCount}, Removed: {removedCount}");
    }
}
