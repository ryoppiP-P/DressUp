//==============================================================================
//  File   : SaveStorage.cs
//  Brief  : セーブデータの保存先を吸収する（WebGLはファイルI/Oが使えないため）
//==============================================================================
using System.IO;
using UnityEngine;

public static class SaveStorage {
#if UNITY_WEBGL && !UNITY_EDITOR
    private const string PrefsKey = "save_dat";

    public static bool Exists(string path) => PlayerPrefs.HasKey(PrefsKey);

    public static string Read(string path) => PlayerPrefs.GetString(PrefsKey, "");

    public static void Write(string path, string contents) {
        PlayerPrefs.SetString(PrefsKey, contents);
        PlayerPrefs.Save();   // これを呼ばないと IndexedDB へ書き出されない
    }

    public static void Delete(string path) {
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
    }
#else
    public static bool Exists(string path) => File.Exists(path);

    public static string Read(string path) => File.ReadAllText(path);

    public static void Write(string path, string contents) => File.WriteAllText(path, contents);

    public static void Delete(string path) {
        if (File.Exists(path)) File.Delete(path);
    }
#endif
}
