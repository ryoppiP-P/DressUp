//==============================================================================
//  File   : FairySaveBridge.cs
//  Brief  : 妖精の育成・名簿セーブデータへのアクセス窓口
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  DressUpSaveBridge と同じ方針で、SaveManager 本体(Shift-JIS)を触らずに
//  static ヘルパー側へ処理をまとめる。
//  育成時間は実時間(DateTime.UtcNow)で数えるため、アプリを閉じている間も進む。
//==============================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class FairySaveBridge {
    /// <summary>畑の種スロット数(シーン側の3スロットに対応)</summary>
    public const int SlotCount = 3;

    private static FairySaveData Data {
        get {
            if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return null;
            return SaveManager.Instance.Current.fairyData;
        }
    }

    //==========================================================================
    // 種スロット
    //==========================================================================

    /// <summary>スロットを取得する(足りなければ空スロットを作って埋める)</summary>
    public static SeedSlotData GetSlot(int slotIndex) {
        var data = Data;
        if (data == null || slotIndex < 0 || slotIndex >= SlotCount) return null;

        while (data.slots.Count < SlotCount) data.slots.Add(new SeedSlotData());
        return data.slots[slotIndex];
    }

    public static bool IsPlanted(int slotIndex) {
        var slot = GetSlot(slotIndex);
        return slot != null && slot.isPlanted;
    }

    /// <summary>空いているスロット番号を返す(無ければ -1)</summary>
    public static int FindEmptySlot() {
        for (int i = 0; i < SlotCount; i++) {
            if (!IsPlanted(i)) return i;
        }
        return -1;
    }

    /// <summary>種を植える。keywords / personality は願いを込める時に決まったものを渡す。</summary>
    public static void PlantSeed(int slotIndex, float growSeconds, List<string> keywords, PersonalitySnapshot personality) {
        var slot = GetSlot(slotIndex);
        if (slot == null || slot.isPlanted) return;

        slot.isPlanted = true;
        slot.plantedAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        slot.growSeconds = growSeconds;
        slot.reducedSeconds = 0f;
        slot.keywords = keywords != null ? new List<string>(keywords) : new List<string>();
        slot.personality = personality ?? new PersonalitySnapshot();

        Save();
    }

    /// <summary>残り秒数(0未満にはならない)</summary>
    public static float GetRemainingSeconds(int slotIndex) {
        var slot = GetSlot(slotIndex);
        if (slot == null || !slot.isPlanted) return 0f;

        return Mathf.Max(0f, slot.growSeconds - (float)ElapsedSeconds(slot));
    }

    /// <summary>育ちきったか</summary>
    public static bool IsReadyToHatch(int slotIndex) {
        var slot = GetSlot(slotIndex);
        if (slot == null || !slot.isPlanted) return false;
        return ElapsedSeconds(slot) >= slot.growSeconds;
    }

    /// <summary>時短アイテムで指定秒数だけ縮める</summary>
    public static void ReduceSeconds(int slotIndex, float seconds) {
        var slot = GetSlot(slotIndex);
        if (slot == null || !slot.isPlanted || seconds <= 0f) return;

        slot.reducedSeconds += seconds;
        Save();
    }

    /// <summary>植えてからの経過秒数(実時間 + 時短アイテム分)</summary>
    private static double ElapsedSeconds(SeedSlotData slot) {
        DateTime planted;
        bool parsed = DateTime.TryParse(
            slot.plantedAtUtc, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out planted);
        if (!parsed) return slot.reducedSeconds;

        // "o" 書式なら Kind まで復元されるが、念のため UTC に揃えてから引く
        double real = (DateTime.UtcNow - planted.ToUniversalTime()).TotalSeconds;
        if (real < 0d) real = 0d; // 端末の時計を巻き戻された場合の保険

        return real + slot.reducedSeconds;
    }

    //==========================================================================
    // 誕生・名簿
    //==========================================================================

    /// <summary>
    /// 育ちきったスロットから妖精を1体生み、名簿に登録してスロットを空に戻す。
    /// 生まれた直後は namingDone = false なので、名前を付けるまで街には出ない。
    /// </summary>
    public static FairyRosterEntry HatchSlot(int slotIndex) {
        var data = Data;
        var slot = GetSlot(slotIndex);
        if (data == null || slot == null || !slot.isPlanted) return null;

        string id = string.Format("charaID_{0:0000}", data.nextCharaNumber);
        data.nextCharaNumber++;

        var entry = new FairyRosterEntry {
            characterId = id,
            bornAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            personality = slot.personality ?? new PersonalitySnapshot(),
            namingDone = false,
        };
        data.roster.Add(entry);

        // 誕生日は着せ替え画面の BirthdayView が読むので、キャラ側のセーブにも書いておく
        var now = DateTime.Now;
        SaveManager.Instance.Current.dressUp.GetOrCreate(id).birthDate =
            new BirthDate(now.Year, now.Month, now.Day);

        // スロットを空に戻す
        data.slots[slotIndex] = new SeedSlotData();

        Save();
        return entry;
    }

    public static FairyRosterEntry FindRoster(string characterId) {
        var data = Data;
        if (data == null || string.IsNullOrEmpty(characterId)) return null;
        return data.roster.Find(x => x.characterId == characterId);
    }

    /// <summary>名前がまだ付いていない(誕生フロー途中の)妖精。無ければ null。</summary>
    public static FairyRosterEntry FindUnnamed() {
        var data = Data;
        if (data == null) return null;
        return data.roster.Find(x => !x.namingDone);
    }

    /// <summary>名前を付け終わった印を付ける(これで街に出るようになる)</summary>
    public static void MarkNamingDone(string characterId) {
        var entry = FindRoster(characterId);
        if (entry == null || entry.namingDone) return;

        entry.namingDone = true;
        Save();
    }

    /// <summary>街に出す対象(名前が付いた妖精)の一覧</summary>
    public static List<FairyRosterEntry> GetNamedFairies() {
        var result = new List<FairyRosterEntry>();
        var data = Data;
        if (data == null) return result;

        foreach (var entry in data.roster) {
            if (entry != null && entry.namingDone) result.Add(entry);
        }
        return result;
    }

    private static void Save() {
        if (SaveManager.Instance != null) SaveManager.Instance.SaveAuto();
    }
}
