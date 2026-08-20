using System.Collections.Generic;

// 保存したコーデ1件分。
// アクセサリは同じカテゴリに複数入るので、カテゴリごとにリストで持つ。
public class SavedOutfit {
    public Dictionary<CategoryType, List<DressUpItem>> items = new();

    public void Capture(Dictionary<CategoryType, List<DressUpItem>> equipped) {
        items.Clear();
        foreach (var pair in equipped)
            items[pair.Key] = new List<DressUpItem>(pair.Value);
    }

    /// <summary>着ているものを順番どおりに全部返す</summary>
    public IEnumerable<DressUpItem> AllItems() {
        foreach (var pair in items)
            foreach (var item in pair.Value)
                if (item != null) yield return item;
    }
}
