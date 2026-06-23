// セーブデータ入ってる場所を開くショートカット
using UnityEditor;
using UnityEngine;

public static class OpenPersistentDataPath {
    [MenuItem("Tools/Open Persistent Data Path")]
    public static void Open() {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    [MenuItem("Tools/Delete Save File")]
    public static void DeleteSave() {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "save.dat");
        if (System.IO.File.Exists(path)) {
            System.IO.File.Delete(path);
            Debug.Log($"[Save] Deleted: {path}");
        }
        else {
            Debug.Log("[Save] No save file found");
        }
    }
}
