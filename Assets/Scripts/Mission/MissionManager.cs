//==============================================================================
//  File   : MissionManager.cs
//  Brief  : ミッションの進捗管理
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/6/18
//------------------------------------------------------------------------------
//==============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionManager : MonoBehaviour {
    public static MissionManager Instance { get; private set; }

    [Header("全ミッション定義（Daily/Weekly/Challenge 全部入れる）")]
    [SerializeField] private List<MissionData> allMissions = new();

    // 進捗が変わったらUIに通知
    public event Action OnMissionChanged;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // 常駐させたいなら（シーン跨ぎで進捗を保つ）
        // DontDestroyOnLoad(gameObject);
    }

    void Start() {
        CheckResets();                 // 日付が変わっていたらリセット
        Report(MissionType.Login, 1);  // 起動＝ログイン1回
    }

    //--------------------------------------------------------------
    // 進捗の報告（各機能からここを呼ぶ）
    //--------------------------------------------------------------
    public void Report(MissionType type, int amount = 1) {
        if (SaveManager.Instance == null) return;
        bool changed = false;

        foreach (var m in allMissions.Where(m => m.type == type)) {
            var e = GetEntry(m.missionId);
            if (e.claimed) continue;                  // 受取済みは進めない
            if (e.progress >= m.targetCount) continue; // 既に達成済みなら進めない

            e.progress = Mathf.Min(e.progress + amount, m.targetCount);
            changed = true;
        }

        if (changed) {
            UpdateClearAllMissions(); // 「全部クリア」系を再評価
            Save();
            OnMissionChanged?.Invoke();
        }
    }

    //--------------------------------------------------------------
    // 状態の取得
    //--------------------------------------------------------------
    public MissionState GetState(MissionData m) {
        var e = GetEntry(m.missionId);
        if (e.claimed) return MissionState.Claimed;
        if (e.progress >= m.targetCount) return MissionState.Claimable;
        return MissionState.InProgress;
    }

    public int GetProgress(MissionData m) => GetEntry(m.missionId).progress;

    public List<MissionData> GetMissions(MissionCategory category) =>
        allMissions.Where(m => m.category == category).ToList();

    //--------------------------------------------------------------
    // 報酬の受け取り
    //--------------------------------------------------------------
    public bool Claim(MissionData m) {
        var e = GetEntry(m.missionId);
        if (e.claimed) return false;
        if (e.progress < m.targetCount) return false; // まだ達成してない

        // 報酬付与
        if (m.rewardNut > 0) SaveManager.Instance.AddCurrency(CurrencyType.Nut, m.rewardNut);
        if (m.rewardHoney > 0) SaveManager.Instance.AddCurrency(CurrencyType.Honey, m.rewardHoney);

        e.claimed = true;
        Save();
        OnMissionChanged?.Invoke();
        return true;
    }

    // 一括受取（そのカテゴリの受取可能を全部）
    public void ClaimAll(MissionCategory category) {
        foreach (var m in GetMissions(category)) {
            if (GetState(m) == MissionState.Claimable)
                Claim(m);
        }
    }

    //--------------------------------------------------------------
    // 「全部クリアしよう」系の自動更新
    //--------------------------------------------------------------
    private void UpdateClearAllMissions() {
        // Daily 全クリア判定
        UpdateClearAll(MissionType.ClearAllDaily, MissionCategory.Daily);
        UpdateClearAll(MissionType.ClearAllWeekly, MissionCategory.Weekly);
    }

    private void UpdateClearAll(MissionType clearAllType, MissionCategory category) {
        // 対象カテゴリの「全部クリア以外」のミッションが全部達成済みか
        var others = allMissions.Where(m => m.category == category && m.type != clearAllType).ToList();
        if (others.Count == 0) return;

        bool allDone = others.All(m => GetEntry(m.missionId).progress >= m.targetCount);

        foreach (var m in allMissions.Where(m => m.type == clearAllType)) {
            var e = GetEntry(m.missionId);
            if (e.claimed) continue;
            e.progress = allDone ? m.targetCount : 0;
        }
    }

    //--------------------------------------------------------------
    // デイリー / ウィークリーのリセット
    //--------------------------------------------------------------
    private void CheckResets() {
        var save = SaveManager.Instance.Current.missionData;
        DateTime now = DateTime.Now;

        // デイリー：日付が変わっていたらリセット
        DateTime lastDaily = ParseDate(save.lastDailyReset);
        if (now.Date > lastDaily.Date) {
            ResetCategory(MissionCategory.Daily);
            save.lastDailyReset = now.ToString("o");
        }

        // ウィークリー：週(月曜始まり)が変わっていたらリセット
        DateTime lastWeekly = ParseDate(save.lastWeeklyReset);
        if (WeekStart(now) > WeekStart(lastWeekly)) {
            ResetCategory(MissionCategory.Weekly);
            save.lastWeeklyReset = now.ToString("o");
        }

        Save();
    }

    private void ResetCategory(MissionCategory category) {
        foreach (var m in GetMissions(category)) {
            var e = GetEntry(m.missionId);
            e.progress = 0;
            e.claimed = false;
        }
    }

    private DateTime ParseDate(string s) {
        if (string.IsNullOrEmpty(s)) return DateTime.MinValue;
        return DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d)
            ? d : DateTime.MinValue;
    }

    private DateTime WeekStart(DateTime d) {
        // 月曜始まりの週頭を返す
        int diff = ((int)d.DayOfWeek + 6) % 7; // 月=0
        return d.Date.AddDays(-diff);
    }

    //--------------------------------------------------------------
    // セーブ内エントリの取得（無ければ作る）
    //--------------------------------------------------------------
    private MissionSaveEntry GetEntry(string missionId) {
        var list = SaveManager.Instance.Current.missionData.entries;
        var e = list.Find(x => x.missionId == missionId);
        if (e == null) {
            e = new MissionSaveEntry { missionId = missionId, progress = 0, claimed = false };
            list.Add(e);
        }
        return e;
    }

    private void Save() {
        // 既存のセーブ書き出しに合わせて呼ぶ（例：SaveAuto など）
        SaveManager.Instance.SaveAuto();
    }
}
