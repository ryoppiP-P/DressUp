// Assets/Scripts/Editor/BuildSceneDrawer.cs
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using UnityEngine;

[CustomPropertyDrawer(typeof(BuildSceneAttribute))]
public class BuildSceneDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.propertyType != SerializedPropertyType.String) {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // Build Settings に登録され、有効になっているシーン名を集める
        string[] sceneNames = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path))
            .ToArray();

        if (sceneNames.Length == 0) {
            EditorGUI.LabelField(position, label.text, "(Build Settings にシーン無し)");
            return;
        }

        // 現在値のインデックスを探す
        int index = System.Array.IndexOf(sceneNames, property.stringValue);
        if (index < 0) index = 0;

        index = EditorGUI.Popup(position, label.text, index, sceneNames);
        property.stringValue = sceneNames[index];
    }
}
#endif
