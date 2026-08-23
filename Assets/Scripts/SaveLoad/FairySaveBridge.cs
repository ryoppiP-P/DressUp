//==============================================================================
//  File   : FairySaveBridge.cs
//  Brief  : 妖精の育成・名簿セーブデータへのアクセス窓口
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
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

        string id = NextCharacterId();

        var entry = new FairyRosterEntry {
            characterId = id,
            bornAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            personality = slot.personality ?? new PersonalitySnapshot(),
            namingDone = false,
            bornSlotIndex = slotIndex,   // どの鉢から生まれたか(畑で「生まれた！」を出す鉢)
        };
        data.roster.Add(entry);

        // 番号を使い回した場合、前にその番号を使っていたコの着せ替え記録が
        // 残っていることがあるので、生まれたてに引き継がれないよう消しておく。
        var dress = SaveManager.Instance.Current.dressUp.GetOrCreate(id);
        dress.equipped.Clear();
        dress.characterName = "";

        // 誕生日は着せ替え画面の BirthdayView が読むので、キャラ側のセーブにも書いておく
        var now = DateTime.Now;
        dress.birthDate = new BirthDate(now.Year, now.Month, now.Day);

        // スロットを空に戻す
        data.slots[slotIndex] = new SeedSlotData();

        Save();
        return entry;
    }

    private const string IdPrefix = "charaID_";

    /// <summary>
    /// 次に生まれる妖精の characterId を決める。
    /// 見るのは名簿(実際にいるコ)だけで、空いている一番小さい番号を使う。
    /// nextCharaNumber には引きずられないので、まっさらなセーブなら必ず 0001 から。
    ///
    /// 着せ替えセーブは見ない。着せ替え画面を開いただけで空の記録が作られるため、
    /// それを「使用済み」と数えると誰もいないのに番号だけ進んでしまう
    /// (実際にこれで 0001/0002 が飛ばされて 0003 から始まっていた)。
    /// 番号を使い回した時に前の記録が残らないよう、HatchSlot 側で消してから配る。
    /// </summary>
    private static string NextCharacterId() {
        var data = Data;
        if (data == null) return IdPrefix + "0001";

        var used = new HashSet<int>();
        foreach (var entry in data.roster) {
            if (entry == null) continue;

            int number = ParseIdNumber(entry.characterId);
            if (number > 0) used.Add(number);
        }

        int next = 1;
        while (used.Contains(next)) next++;

        data.nextCharaNumber = next + 1; // 記録用(採番はここを見ていない)
        return IdPrefix + next.ToString("0000");
    }

    /// <summary>"charaID_0004" → 4。読めなければ 0。</summary>
    private static int ParseIdNumber(string characterId) {
        if (string.IsNullOrEmpty(characterId)) return 0;
        if (!characterId.StartsWith(IdPrefix)) return 0;

        int number;
        return int.TryParse(characterId.Substring(IdPrefix.Length), out number) ? number : 0;
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
