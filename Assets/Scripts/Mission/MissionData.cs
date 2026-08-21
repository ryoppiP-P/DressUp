//==============================================================================
//  File   : MissionData.cs
//  Brief  : ミッションデータの1件分の定義
// 
//  Author : Ryoto Kikuchi
//  Date   : 2026/7/18
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;

[CreateAssetMenu(menuName = "Mission/MissionData")]
public class MissionData : ScriptableObject {
    [Header("識別")]
    public string missionId;              // セーブのキー（不変・ユニーク）
    public MissionCategory category;      // Daily / Weekly / Challenge
    public MissionType type;              // 進捗カウント対象

    [Header("表示")]
    [TextArea] public string description; // 「街に装飾を置こう」など

    [Header("達成条件")]
    public int targetCount = 1;           // 目標回数（例：装飾を2個で 2）

    [Header("報酬")]
    public int rewardNut = 0;             // どんぐり
    public int rewardHoney = 0;           // はちみつ

    // 段階制のミッション(チャレンジ)用。
    // 1件でも入れるとこちらが優先され、上の targetCount / reward は使われない。
    // 「5回で400 → 10回で500 → …」のように、達成するたびに次の段階へ進む。
    [System.Serializable]
    public class Tier {
        public int targetCount = 1;
        public int rewardNut = 0;
        public int rewardHoney = 0;
    }

    [Header("段階(チャレンジ用。空なら上の単発の条件を使う)")]
    public System.Collections.Generic.List<Tier> tiers = new System.Collections.Generic.List<Tier>();

    /// <summary>段階制かどうか</summary>
    public bool IsStaged => tiers != null && tiers.Count > 0;

    /// <summary>段階の数(段階制でなければ1)</summary>
    public int StageCount => IsStaged ? tiers.Count : 1;

    /// <summary>その段階の目標回数</summary>
    public int GetTarget(int stage) {
        if (!IsStaged) return targetCount;

        stage = Mathf.Clamp(stage, 0, tiers.Count - 1);
        return tiers[stage].targetCount;
    }

    /// <summary>最後の段階の目標回数(進捗の上限)</summary>
    public int FinalTarget => IsStaged ? tiers[tiers.Count - 1].targetCount : targetCount;

    public int GetRewardNut(int stage) {
        if (!IsStaged) return rewardNut;

        stage = Mathf.Clamp(stage, 0, tiers.Count - 1);
        return tiers[stage].rewardNut;
    }

    public int GetRewardHoney(int stage) {
        if (!IsStaged) return rewardHoney;

        stage = Mathf.Clamp(stage, 0, tiers.Count - 1);
        return tiers[stage].rewardHoney;
    }
}