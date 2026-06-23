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
    public List<EquippedEntry> equipped = new List<EquippedEntry>();    // 現在装備
    public List<SavedOutfitData> savedOutfits = new List<SavedOutfitData>();    // 保存コーデ
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