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
    public StageProgressData stageProgress = new StageProgressData();
    public PlayerData playerData = new PlayerData();
    public StageSelectData stageSelect = new StageSelectData();
    public ArchiveData archive = new ArchiveData();           // 記録保管所
    public SettingsData settings = new SettingsData();
    public CompanionCustomData companionCustom = new CompanionCustomData();
}

// ステージクリア状況・星
[Serializable]
public class StageProgressData {
    public List<StageRecord> stages = new List<StageRecord>();

    //--------------------------------------------------------------------------
    // 取得・検索
    //--------------------------------------------------------------------------

    public StageRecord GetOrCreate(string stageId) {
        var r = stages.Find(s => s.stageId == stageId);
        if (r == null) {
            r = new StageRecord { stageId = stageId };
            stages.Add(r);
        }
        return r;
    }

    // クリア済みか
    public bool IsCleared(string stageId) {
        var rec = stages.Find(s => s.stageId == stageId);
        return rec != null && rec.cleared;
    }

    // 星取得済みか
    public bool IsStarCollected(string stageId, int starIndex) {
        var rec = stages.Find(s => s.stageId == stageId);
        if (rec == null) return false;
        if (starIndex < 0 || starIndex >= rec.starsCollected.Length) return false;
        return rec.starsCollected[starIndex];
    }

    // 星の取得数
    public int GetStarCount(string stageId) {
        var rec = stages.Find(s => s.stageId == stageId);
        if (rec == null) return 0;
        int count = 0;
        foreach (var s in rec.starsCollected) if (s) count++;
        return count;
    }

    //--------------------------------------------------------------------------
    // 更新
    //--------------------------------------------------------------------------

    public void SetCleared(string stageId) {
        GetOrCreate(stageId).cleared = true;
    }

    public void SetStarCollected(string stageId, int starIndex) {
        var rec = GetOrCreate(stageId);
        if (starIndex >= 0 && starIndex < rec.starsCollected.Length) {
            rec.starsCollected[starIndex] = true;
        }
    }
}


[Serializable]
public class StageRecord {
    public string stageId;          // "Stage1-1", "Stage2-4" など
    public bool cleared;
    public bool[] starsCollected = new bool[3]; // 星3つ
}

// プレイヤー全体（コイン等）
[Serializable]
public class PlayerData {
    public int coinCount;
}

// ステージ選択画面の現在位置
[Serializable]
public class StageSelectData {
    public string currentStageId = "1-1"; // ワールドマップ上の現在地
    public float mapPositionX;
    public float mapPositionY;
}

// 記録保管所
[Serializable]
public class ArchiveData {
    public List<string> unlockedEntries = new List<string>();
}

// 設定
[Serializable]
public class SettingsData {
    public float masterVolume = 70f;
    public float bgmVolume = 50f;
    public float seVolume = 50f;
    public int screenMode = 0; // 0: フルスクリーン, 1: ボーダーレス, 2: ウィンドウ
}

// お供カスタマイズ
[Serializable]
public class CompanionCustomData {
    public string skinId = "default";
    public List<string> unlockedSkins = new List<string>();

    public bool IsSkinUnlocked(string colorId) => unlockedSkins.Contains(colorId);
    public void UnlockSkin(string colorId) {
        if (!unlockedSkins.Contains(colorId)) unlockedSkins.Add(colorId);
    }
}
