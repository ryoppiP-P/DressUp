using System.Collections.Generic;

// カテゴリごとの着用リスト。
// 服やヘアは1つだけだが、アクセサリ(髪飾り/メガネ/体アクセ)は最大4つまで入る。
// 上限は CategoryMap.GetMaxEquip() が持っている。
public class EquipState {
    public Dictionary<CategoryType, List<DressUpItem>> equipped = new();

    /// <summary>そのカテゴリで着ているもの全部(着ていなければ空)</summary>
    public List<DressUpItem> GetAll(CategoryType category) {
        return equipped.TryGetValue(category, out var list) && list != null
            ? list
            : new List<DressUpItem>();
    }

    /// <summary>そのカテゴリの1つ目(着ていなければ null)</summary>
    public DressUpItem Get(CategoryType category) {
        return equipped.TryGetValue(category, out var list) && list != null && list.Count > 0
            ? list[0]
            : null;
    }

    public bool Contains(CategoryType category, DressUpItem item) {
        return item != null && equipped.TryGetValue(category, out var list) && list != null && list.Contains(item);
    }

    public int Count(CategoryType category) {
        return equipped.TryGetValue(category, out var list) && list != null ? list.Count : 0;
    }

    /// <summary>そのカテゴリをこの1つだけにする(従来の Set と同じ挙動)</summary>
    public void Set(CategoryType category, DressUpItem item) {
        if (item == null) { Clear(category); return; }
        equipped[category] = new List<DressUpItem> { item };
    }

    /// <summary>着ているものに追加する。上限を超える時は一番古いものを外す。</summary>
    public void Add(CategoryType category, DressUpItem item) {
        if (item == null) return;

        if (!equipped.TryGetValue(category, out var list) || list == null) {
            list = new List<DressUpItem>();
            equipped[category] = list;
        }
        if (list.Contains(item)) return;

        list.Add(item);

        int max = CategoryMap.GetMaxEquip(category);
        while (list.Count > max) list.RemoveAt(0); // 一番古いものから外れる
    }

    /// <summary>その1つだけ外す</summary>
    public void Remove(CategoryType category, DressUpItem item) {
        if (item == null) return;
        if (!equipped.TryGetValue(category, out var list) || list == null) return;

        list.Remove(item);
        if (list.Count == 0) equipped.Remove(category);
    }

    public void Clear(CategoryType category) {
        equipped.Remove(category);
    }

    public void ClearAll() {
        equipped.Clear();
    }
}
