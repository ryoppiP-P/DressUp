//==============================================================================
//  File   : ConsumableBridge.cs
//  Brief  : 使うと減るアイテム(種・時短の実)の所持数へのアクセス窓口
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/23
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;

public static class ConsumableBridge {
    private static ConsumableSaveData Data {
        get {
            if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return null;
            return SaveManager.Instance.Current.consumables;
        }
    }

    /// <summary>その消耗品を何個持っているか</summary>
    public static int GetCount(string itemId) {
        var entry = Find(itemId);
        return entry != null ? entry.count : 0;
    }

    public static int GetCount(GameItem item) {
        return item != null ? GetCount(item.itemId) : 0;
    }

    public static bool Has(string itemId, int amount = 1) {
        return GetCount(itemId) >= amount;
    }

    /// <summary>増やす(ショップで買った時など)</summary>
    public static void Add(string itemId, int amount = 1) {
        if (string.IsNullOrEmpty(itemId) || amount <= 0) return;

        var data = Data;
        if (data == null) return;

        var entry = Find(itemId);
        if (entry == null) {
            entry = new ConsumableEntry { itemId = itemId, count = 0 };
            data.items.Add(entry);
        }

        entry.count += amount;
        Save();
    }

    /// <summary>使う。足りなければ false を返して何もしない。</summary>
    public static bool TryConsume(string itemId, int amount = 1) {
        if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;

        var entry = Find(itemId);
        if (entry == null || entry.count < amount) return false;

        entry.count -= amount;
        if (entry.count <= 0) Data.items.Remove(entry);

        Save();
        return true;
    }

    public static bool TryConsume(GameItem item, int amount = 1) {
        return item != null && TryConsume(item.itemId, amount);
    }

    private static ConsumableEntry Find(string itemId) {
        var data = Data;
        if (data == null || string.IsNullOrEmpty(itemId)) return null;

        return data.items.Find(x => x != null && x.itemId == itemId);
    }

    private static void Save() {
        if (SaveManager.Instance != null) SaveManager.Instance.SaveAuto();
    }
}
