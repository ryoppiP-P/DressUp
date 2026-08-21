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

    [Header("「服を集めよう」で数えるアイテムデータベース")]
    [SerializeField] private ItemDatabase itemDatabase;

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
        SyncDerivedProgress();         // セーブを見れば分かる進捗を合わせる
        Report(MissionType.Login, 1);  // 起動＝ログイン1回
    }

    //--------------------------------------------------------------
    // セーブの中身から決まる進捗の反映
    //--------------------------------------------------------------
    // 誕生した妖精の数 / 持っている服の数 / 累計プレイ時間 は、
    // ミッション画面とは別のシーンで増えることがあり、その場で Report できない。
    // (MissionManager は街のシーンにしか居ないため)
    // なので「開いた時にセーブから数え直す」形にしている。
    public void SyncDerivedProgress() {
        if (SaveManager.Instance == null) return;

        bool changed = false;
        changed |= SetProgress(MissionType.BornFairy, CountBornFairies());
        changed |= SetProgress(MissionType.CollectClothes, CountOwnedClothes());
        changed |= SetProgress(MissionType.PlayTime, PlayTimeTracker.TotalMinutes);
        changed |= SetProgress(MissionType.MakePersonality, CountPersonalityKinds());

        if (!changed) return;

        UpdateClearAllMissions();
        Save();
        OnMissionChanged?.Invoke();
    }

    // その種類のミッションの進捗を、指定の値に合わせる(受取済みは触らない)
    private bool SetProgress(MissionType type, int value) {
        bool changed = false;

        foreach (var m in allMissions.Where(m => m.type == type)) {
            var e = GetEntry(m.missionId);
            if (IsAllDone(m, e)) continue;

            int clamped = Mathf.Clamp(value, 0, m.FinalTarget);
            if (e.progress == clamped) continue;

            e.progress = clamped;
            changed = true;
        }
        return changed;
    }

    // 今までに生まれた妖精の数(名簿は誕生後ずっと残るのでそのまま数えられる)
    private int CountBornFairies() {
        var data = SaveManager.Instance.Current.fairyData;
        return data != null && data.roster != null ? data.roster.Count : 0;
    }

    // 今までに作った妖精の「性格の種類」の数。
    // 一番大きい軸をそのコの性格とみなし、何種類ぶん揃ったかを数える(最大6)。
    private int CountPersonalityKinds() {
        var data = SaveManager.Instance.Current.fairyData;
        if (data == null || data.roster == null) return 0;

        var kinds = new HashSet<PersonalityAxis>();
        foreach (var entry in data.roster) {
            if (entry == null || entry.personality == null) continue;
            kinds.Add(DominantAxis(entry.personality));
        }
        return kinds.Count;
    }

    // 一番値が大きい軸を、そのコの性格とみなす(同点なら定義順で先のもの)
    private static PersonalityAxis DominantAxis(PersonalitySnapshot personality) {
        var best = PersonalityAxis.Mystery;
        int bestValue = int.MinValue;

        foreach (PersonalityAxis axis in System.Enum.GetValues(typeof(PersonalityAxis))) {
            int value = personality.Get(axis);
            if (value <= bestValue) continue;

            bestValue = value;
            best = axis;
        }
        return best;
    }

    // 持っている着せ替えアイテムの数(服・アクセサリ・目・口を全部数える)
    private int CountOwnedClothes() {
        if (itemDatabase == null || itemDatabase.allItems == null) return 0;

        int count = 0;
        foreach (var item in itemDatabase.allItems) {
            if (item == null) continue;
            if (SaveManager.Instance.IsItemOwned(item)) count++;
        }
        return count;
    }

    //--------------------------------------------------------------
    // 進捗の報告（各機能からここを呼ぶ）
    //--------------------------------------------------------------
    public void Report(MissionType type, int amount = 1) {
        if (SaveManager.Instance == null) return;
        bool changed = false;

        foreach (var m in allMissions.Where(m => m.type == type)) {
            var e = GetEntry(m.missionId);
            if (IsAllDone(m, e)) continue;              // 全部終わっているものは進めない
            if (e.progress >= m.FinalTarget) continue;  // 上限まで来ていたら進めない

            e.progress = Mathf.Min(e.progress + amount, m.FinalTarget);
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
        if (IsAllDone(m, e)) return MissionState.Claimed;
        if (e.progress >= m.GetTarget(GetStage(m))) return MissionState.Claimable;
        return MissionState.InProgress;
    }

    public int GetProgress(MissionData m) => GetEntry(m.missionId).progress;

    /// <summary>今どの段階に挑戦中か(0始まり)。段階制でなければ常に0。</summary>
    public int GetStage(MissionData m) {
        var e = GetEntry(m.missionId);
        return Mathf.Clamp(e.claimedStage, 0, m.StageCount - 1);
    }

    /// <summary>今の段階の目標回数</summary>
    public int GetTarget(MissionData m) => m.GetTarget(GetStage(m));

    /// <summary>全部の段階を受け取り終わったか</summary>
    private bool IsAllDone(MissionData m, MissionSaveEntry e) {
        return m.IsStaged ? e.claimedStage >= m.StageCount : e.claimed;
    }

    public List<MissionData> GetMissions(MissionCategory category) =>
        allMissions.Where(m => m.category == category).ToList();

    //--------------------------------------------------------------
    // 報酬の受け取り
    //--------------------------------------------------------------
    public bool Claim(MissionData m) {
        var e = GetEntry(m.missionId);
        if (IsAllDone(m, e)) return false;

        int stage = GetStage(m);
        if (e.progress < m.GetTarget(stage)) return false; // まだ達成してない

        // 報酬付与(段階制ならその段階のぶん)
        int nut = m.GetRewardNut(stage);
        int honey = m.GetRewardHoney(stage);
        if (nut > 0) SaveManager.Instance.AddCurrency(CurrencyType.Nut, nut);
        if (honey > 0) SaveManager.Instance.AddCurrency(CurrencyType.Honey, honey);

        if (m.IsStaged) e.claimedStage = stage + 1;  // 次の段階へ進む
        else e.claimed = true;

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

        bool allDone = others.All(m => GetEntry(m.missionId).progress >= m.GetTarget(GetStage(m)));

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
            e.claimedStage = 0;
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
