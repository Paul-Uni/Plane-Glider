#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Tools > Force Reserialize Assets
// Speichert alle Assets im aktuellen Format neu (behebt "below the supported minimum"-Warnungen).
public static class ForceReserialize
{
    [MenuItem("Tools/Force Reserialize Assets")]
    static void Run()
    {
        if (!EditorUtility.DisplayDialog("Force Reserialize",
            "Alle Assets im Projekt neu speichern? Kann je nach Projektgröße etwas dauern.", "Los", "Abbrechen"))
            return;
        AssetDatabase.ForceReserializeAssets();
        Debug.Log("Force Reserialize fertig.");
    }
}
#endif
