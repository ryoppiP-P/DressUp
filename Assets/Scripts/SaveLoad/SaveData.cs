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

[Serializable]
public class SaveData {
    // バージョン管理
    public int saveVersion = 1;

    // 各システム
    public PlayerData playerData = new PlayerData();
    public SettingsData settings = new SettingsData();
    public DressUpSaveData dressUp = new DressUpSaveData();
    public MissionSaveData missionData = new MissionSaveData();
    public ItemSaveData itemData = new ItemSaveData();
    public ConsumableSaveData consumables = new ConsumableSaveData(); // 使うと減るもの(種・時短の実)
    public IntimacySaveData intimacyData = new IntimacySaveData();
    public FairySaveData fairyData = new FairySaveData();
    public TownSaveData townData = new TownSaveData();
}

// プレイヤー全体（コイン等）
[Serializable]
public class PlayerData {
    public int nutCurrency = 0;    // 木の実通貨（仮）
    public int honeyCurrency = 0;  // はちみつ通貨（仮）
}
public enum CurrencyType {
    Nut,    // 木の実
    Honey,  // はちみつ
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

    // コーデ保存
    public List<SavedOutfitData> savedOutfits = new List<SavedOutfitData>();

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
    public string characterName = "";   // キャラの名前
    public List<EquippedEntry> equipped = new List<EquippedEntry>();
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

// ミッション単体のセーブデータ
[System.Serializable]
public class MissionSaveEntry {
    public string missionId;
    public int progress;      // 現在の進捗数
    public bool claimed;      // 受取済みか(単発ミッション用)
    public int claimedStage;  // 受け取り済みの段階数(段階制ミッション用。0=まだ1つも受け取っていない)
}
// ミッション全体のセーブデータ
[System.Serializable]
public class MissionSaveData {
    public List<MissionSaveEntry> entries = new();
    public string lastDailyReset;   // 最終デイリーリセット日時(ISO文字列)
    public string lastWeeklyReset;  // 最終ウィークリーリセット日時(ISO文字列)
    public double playSeconds;      // 累計プレイ時間(秒)。「N時間プレイしよう」で使う
}

// 所持アイテムのセーブデータ
[System.Serializable]
public class ItemSaveData {
    // 所持しているアイテムの itemId 一覧（GameItem.itemId と対応）
    public List<string> ownedItemIds = new List<string>();
}

// キャラクター同士の親密度のセーブデータ(2人1組)
[System.Serializable]
public class IntimacyEntry {
    public string charaIdA;
    public string charaIdB;
    public int value; // 0-100
}
[System.Serializable]
public class IntimacySaveData {
    public List<IntimacyEntry> entries = new List<IntimacyEntry>();
}
