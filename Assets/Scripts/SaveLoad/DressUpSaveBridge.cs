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
            foreach (var item in pair.Value)
                state.Add(pair.Key, item);
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

    // アクセサリは同じカテゴリに複数入るので、1アイテム=1エントリで並べて書く。
    // 保存形式(List<EquippedEntry>)自体は元から複数行を許すので、古いセーブもそのまま読める。
    private static List<EquippedEntry> ToEntries(Dictionary<CategoryType, List<DressUpItem>> dict) {
        var list = new List<EquippedEntry>();
        foreach (var pair in dict) {
            if (pair.Value == null) continue;

            foreach (var item in pair.Value) {
                if (item == null) continue;
                list.Add(new EquippedEntry {
                    category = pair.Key.ToString(),
                    itemName = item.itemName
                });
            }
        }
        return list;
    }

    private static Dictionary<CategoryType, List<DressUpItem>> ToDict(List<EquippedEntry> entries, ItemDatabase db) {
        var dict = new Dictionary<CategoryType, List<DressUpItem>>();
        foreach (var e in entries) {
            var item = db.Find(e.itemName);
            if (item == null) continue;
            if (!System.Enum.TryParse<CategoryType>(e.category, out var cat)) continue;

            if (!dict.TryGetValue(cat, out var list)) {
                list = new List<DressUpItem>();
                dict[cat] = list;
            }

            // 上限を超えている古いセーブが来ても、入る分だけにしておく
            if (list.Count >= CategoryMap.GetMaxEquip(cat)) continue;
            if (!list.Contains(item)) list.Add(item);
        }
        return dict;
    }
}
