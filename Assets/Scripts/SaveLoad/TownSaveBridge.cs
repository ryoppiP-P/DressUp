//==============================================================================
//  File   : TownSaveBridge.cs
//  Brief  : 街のキャラクターの居場所セーブへのアクセス窓口
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  位置は何秒かおきにまとめて書き込む想定なので、
//  SetPosition はメモリ上だけ更新し、Flush で1回だけディスクへ書く形にしてある
//  (親密度の AddIntimacy / FlushIntimacySave と同じ考え方)。
//==============================================================================
using UnityEngine;

public static class TownSaveBridge {

    private static TownSaveData Data {
        get {
            if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return null;
            return SaveManager.Instance.Current.townData;
        }
    }

    /// <summary>前回いた場所を取得する。記録が無ければ false。</summary>
    public static bool TryGetPosition(string characterId, out Vector3 position) {
        position = Vector3.zero;

        var data = Data;
        if (data == null || string.IsNullOrEmpty(characterId)) return false;

        var entry = data.positions.Find(x => x.characterId == characterId);
        if (entry == null) return false;

        position = new Vector3(entry.x, entry.y, 0f);
        return true;
    }

    /// <summary>居場所を記録する(既定ではメモリ上だけ。書き込みは Flush でまとめて)</summary>
    public static void SetPosition(string characterId, Vector3 position, bool immediateSave = false) {
        var data = Data;
        if (data == null || string.IsNullOrEmpty(characterId)) return;

        var entry = data.positions.Find(x => x.characterId == characterId);
        if (entry == null) {
            entry = new CharacterPositionEntry { characterId = characterId };
            data.positions.Add(entry);
        }

        entry.x = position.x;
        entry.y = position.y;

        if (immediateSave) Flush();
    }

    /// <summary>メモリ上に貯めた居場所をまとめてディスクへ書き込む</summary>
    public static void Flush() {
        if (SaveManager.Instance != null) SaveManager.Instance.SaveAuto();
    }

    /// <summary>記録を消す(そのキャラは次回ランダム配置になる)</summary>
    public static void ClearPosition(string characterId) {
        var data = Data;
        if (data == null || string.IsNullOrEmpty(characterId)) return;

        data.positions.RemoveAll(x => x.characterId == characterId);
    }
}
