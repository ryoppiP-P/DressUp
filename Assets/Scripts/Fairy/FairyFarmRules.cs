//==============================================================================
//  File   : FairyFarmRules.cs
//  Brief  : 妖精の畑のルール(種を使う・時短を使う・作れる数の上限)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/23
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;

public static class FairyFarmRules {
    /// <summary>このゲームで作れるキャラクターの上限</summary>
    public const int MaxCharacters = 5;

    /// <summary>上限に達している時に出すメッセージ</summary>
    public const string LimitMessage = "これ以上作れません！\nアップデートをお待ちください！";

    public const string NoSeedMessage = "種が足りないよ";
    public const string NoReduceItemMessage = "時短の実を持っていないよ";

    //--------------------------------------------------------------------------
    // 使うアイテム(Inspectorで差し替えられるよう、参照は画面側が持つ)
    //--------------------------------------------------------------------------

    /// <summary>種を何個持っているか</summary>
    public static int SeedCount(OtherItem seedItem) {
        return ConsumableBridge.GetCount(seedItem);
    }

    /// <summary>時短の実を何個持っているか</summary>
    public static int ReduceItemCount(OtherItem reduceItem) {
        return ConsumableBridge.GetCount(reduceItem);
    }

    //--------------------------------------------------------------------------
    // 作れる数の上限
    //--------------------------------------------------------------------------

    /// <summary>
    /// 今このゲームに何体ぶんのキャラがいるか。
    /// 生まれた妖精だけでなく、育ちかけの種も「これから1体になるもの」として数える。
    /// </summary>
    public static int CurrentCharacterCount {
        get {
            if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return 0;

            var fairy = SaveManager.Instance.Current.fairyData;
            int count = fairy != null && fairy.roster != null ? fairy.roster.Count : 0;

            for (int i = 0; i < FairySaveBridge.SlotCount; i++)
                if (FairySaveBridge.IsPlanted(i)) count++;

            return count;
        }
    }

    public static bool IsFull {
        get { return CurrentCharacterCount >= MaxCharacters; }
    }

    //--------------------------------------------------------------------------
    // 種を植える
    //--------------------------------------------------------------------------

    /// <summary>植えられるか。ダメな時は理由を返す。</summary>
    public static bool CanPlant(OtherItem seedItem, out string reason) {
        if (IsFull) { reason = LimitMessage; return false; }
        if (SeedCount(seedItem) <= 0) { reason = NoSeedMessage; return false; }

        reason = "";
        return true;
    }

    /// <summary>
    /// 種を1つ使う。使えたら true。
    /// 実際に植える処理(スロットへの書き込み)は呼び出し側で行う。
    /// </summary>
    public static bool TryUseSeed(OtherItem seedItem, out string reason) {
        if (!CanPlant(seedItem, out reason)) return false;

        if (!ConsumableBridge.TryConsume(seedItem, 1)) {
            reason = NoSeedMessage;
            return false;
        }
        return true;
    }

    //--------------------------------------------------------------------------
    // 時間を短縮する
    //--------------------------------------------------------------------------

    /// <summary>
    /// 時短の実を1つ使って、そのスロットの残り時間を縮める。
    /// 持っていなければ false(何も起きない)。
    /// </summary>
    public static bool TryReduceTime(OtherItem reduceItem, int slotIndex, out string reason) {
        reason = "";

        if (!FairySaveBridge.IsPlanted(slotIndex)) { reason = ""; return false; }
        if (ReduceItemCount(reduceItem) <= 0) { reason = NoReduceItemMessage; return false; }

        float seconds = ResolveReduceSeconds(reduceItem);
        if (!ConsumableBridge.TryConsume(reduceItem, 1)) { reason = NoReduceItemMessage; return false; }

        FairySaveBridge.ReduceSeconds(slotIndex, seconds);
        return true;
    }

    // 縮む秒数はアイテム側の設定を使う(TimeReduceItem なら reduceSeconds)
    private static float ResolveReduceSeconds(OtherItem reduceItem) {
        var timeItem = reduceItem as TimeReduceItem;
        return timeItem != null ? timeItem.reduceSeconds : 1800f; // 保険で30分
    }
}
