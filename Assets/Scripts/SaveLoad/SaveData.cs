//==============================================================================
//  File   : SaveData.cs
//  Brief  : セーブデータの管理
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/6/18
//------------------------------------------------------------------------------
//==============================================================================
using System;
using System.Collections.Generic;
using static Character;

[Serializable]
public class SaveData {
    // バージョン管理
    public int saveVersion = 1;

    // 各システム
    public PlayerData playerData = new PlayerData();
    public SettingsData settings = new SettingsData();
    public DressUpSaveData dressUp = new DressUpSaveData();
}

// プレイヤー全体（コイン等）
[Serializable]
public class PlayerData {
    public int coinCount;
}

// 設定
[Serializable]
public class SettingsData {
    public float masterVolume = 70f;
    public float bgmVolume = 50f;
    public float seVolume = 50f;
}

// 着せ替え関連のセーブデータ
[Serializable]
public class DressUpSaveData {
    public List<CharacterDressData> characters = new List<CharacterDressData>();

    // ID から取得（無ければ作る）
    public CharacterDressData GetOrCreate(string characterId) {
        var c = characters.Find(x => x.characterId == characterId);
        if (c == null) {
            c = new CharacterDressData { characterId = characterId };
            characters.Add(c);
        }
        return c;
    }
}

[Serializable]
public class CharacterDressData {
    public string characterId;
    public List<EquippedEntry> equipped = new List<EquippedEntry>();
    public List<SavedOutfitData> savedOutfits = new List<SavedOutfitData>();
    public BirthDate birthDate = new BirthDate();
}

[System.Serializable]
public class BirthDate {
    public int year;
    public int month;
    public int day;

    public BirthDate() { }
    public BirthDate(int y, int m, int d) { year = y; month = m; day = d; }

    // DateTime に変換（無効値なら null）
    public System.DateTime? ToDateTime() {
        try { return new System.DateTime(year, month, day); } catch { return null; }
    }
}

[Serializable]
public class EquippedEntry {
    public string category;
    public string itemName;
}

[Serializable]
public class SavedOutfitData {
    public List<EquippedEntry> items = new List<EquippedEntry>();
}