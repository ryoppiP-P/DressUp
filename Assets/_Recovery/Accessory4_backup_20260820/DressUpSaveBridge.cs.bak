using System.Collections.Generic;

public static class DressUpSaveBridge {

    //==========================================================================
    // 現在の装備
    //==========================================================================

    // 現在の装備を永続化
    public static void SaveEquipped(string characterId) {
        if (SaveManager.Instance == null) return;
        var data = SaveManager.Instance.Current.dressUp.GetOrCreate(characterId);
        data.equipped = ToEntries(SaveManager.Instance.GetEquipState(characterId).equipped);
        SaveManager.Instance.SaveAuto();
    }

    // 永続データを State に復元
    public static void LoadIntoState(string characterId, ItemDatabase db) {
        if (SaveManager.Instance == null || db == null) return;
        var data = SaveManager.Instance.Current.dressUp.GetOrCreate(characterId);
        var state = SaveManager.Instance.GetEquipState(characterId);
        state.ClearAll();
        foreach (var pair in ToDict(data.equipped, db))
            state.Set(pair.Key, pair.Value);
    }

    //==========================================================================
    // 保存コーデ
    //==========================================================================

    // SavedOutfit 1件を末尾に追加して永続化
    public static void AddSavedOutfit(SavedOutfit outfit) {
        if (SaveManager.Instance == null) return;
        var data = SaveManager.Instance.Current.dressUp;
        data.savedOutfits.Add(new SavedOutfitData { items = ToEntries(outfit.items) });
        SaveManager.Instance.SaveAuto();
    }

    // 指定インデックスの保存コーデを削除して永続化
    public static void RemoveSavedOutfit(int index) {
        if (SaveManager.Instance == null) return;
        var data = SaveManager.Instance.Current.dressUp;
        if (index < 0 || index >= data.savedOutfits.Count) return;
        data.savedOutfits.RemoveAt(index);
        SaveManager.Instance.SaveAuto();
    }

    // 保存コーデを全部 SavedOutfit のリストに復元して返す
    public static List<SavedOutfit> LoadSavedOutfits(ItemDatabase db) {
        var result = new List<SavedOutfit>();
        if (SaveManager.Instance == null || db == null) return result;
        var data = SaveManager.Instance.Current.dressUp;
        foreach (var od in data.savedOutfits)
            result.Add(new SavedOutfit { items = ToDict(od.items, db) });
        return result;
    }

    //==========================================================================
    // 変換ヘルパー
    //==========================================================================

    private static List<EquippedEntry> ToEntries(Dictionary<CategoryType, DressUpItem> dict) {
        var list = new List<EquippedEntry>();
        foreach (var pair in dict) {
            if (pair.Value == null) continue;
            list.Add(new EquippedEntry {
                category = pair.Key.ToString(),
                itemName = pair.Value.itemName
            });
        }
        return list;
    }

    private static Dictionary<CategoryType, DressUpItem> ToDict(List<EquippedEntry> entries, ItemDatabase db) {
        var dict = new Dictionary<CategoryType, DressUpItem>();
        foreach (var e in entries) {
            var item = db.Find(e.itemName);
            if (item == null) continue;
            if (System.Enum.TryParse<CategoryType>(e.category, out var cat))
                dict[cat] = item;
        }
        return dict;
    }
}
