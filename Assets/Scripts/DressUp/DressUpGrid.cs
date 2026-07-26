using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DressupGrid : MonoBehaviour {
    [SerializeField] private Transform gridContent;     // Grid Layout Group の Content
    [SerializeField] private ItemButton itemButtonPrefab;
    [SerializeField] private ItemDatabase database;     // ScriptableObject で作ったアイテムデータベース

    private readonly List<ItemButton> _spawned = new();

    // 全アイテムはDBから取る（nullや空はここで吸収）
    private IEnumerable<DressUpItem> AllItems =>
        (database != null && database.allItems != null)
            ? database.allItems.Where(i => i != null)
            : Enumerable.Empty<DressUpItem>();

    // 今の対象キャラは DressUpTarget から取る
    private Character character => DressUpTarget.Instance != null
        ? DressUpTarget.Instance.Current : null;

    void OnEnable() {
        if (DressUpTarget.Instance != null)
            DressUpTarget.Instance.OnTargetChanged += OnTargetChanged;
        OnTargetChanged(character); // 開いた時点の対象で一度描画
    }

    void OnDisable() {
        if (DressUpTarget.Instance != null)
            DressUpTarget.Instance.OnTargetChanged -= OnTargetChanged;
    }

    // 対象が変わったら作り直す（引数は使わないが Action<Character> に合わせる）
    void OnTargetChanged(Character _) {
        Rebuild(AllItems);
    }

    // 大分類 + 絞り込み種別 + (必要なら)小分類 で表示
    public void Show(CategoryGroup group, FilterKind kind, CategoryType category = default) {
        var groupCats = CategoryMap.GetCategories(group);

        IEnumerable<DressUpItem> filtered = kind switch {
            FilterKind.Equipped => AllItems.Where(IsEquipped),
            FilterKind.Category => AllItems.Where(i => i.category == category),
            _ => AllItems.Where(i => groupCats.Contains(i.category)), // All
        };

        Rebuild(filtered);
    }

    // 名前検索
    public void ShowByName(string keyword) {
        var filtered = string.IsNullOrEmpty(keyword)
            ? AllItems
            : AllItems.Where(i => i.itemName.Contains(keyword));
        Rebuild(filtered);
    }

    private bool IsEquipped(DressUpItem item) {
        if (SaveManager.Instance == null || character == null) return false;
        return SaveManager.Instance.GetEquipState(character.CharacterId).equipped.ContainsValue(item);
    }

    private void Rebuild(IEnumerable<DressUpItem> filtered) {
        foreach (var b in _spawned) Destroy(b.gameObject);
        _spawned.Clear();
        if (character == null) return;

        foreach (var item in filtered) {
            if (item == null) continue;   // null はスキップ（クラッシュ防止）
            var btn = Instantiate(itemButtonPrefab, gridContent);
            btn.Setup(item, character);
            _spawned.Add(btn);
        }
    }

    public void ApplyFilter(FilterCondition cond) {
        IEnumerable<DressUpItem> q = AllItems;

        // 絞り込み
        if (!string.IsNullOrEmpty(cond.nameKeyword))
            q = q.Where(i => i.itemName.Contains(cond.nameKeyword));
        if (cond.rarities.Count > 0)
            q = q.Where(i => cond.rarities.Contains(i.rarity));
        if (cond.colors.Count > 0)
            q = q.Where(i => i.colors.Any(c => cond.colors.Contains(c)));
        if (cond.releaseYears.Count > 0)
            q = q.Where(i => cond.releaseYears.Contains(i.releaseYear));

        // 並べ替え（6択）
        q = cond.sort switch {
            SortOption.ReleaseNew => q.OrderByDescending(i => i.releaseYear),
            SortOption.ReleaseOld => q.OrderBy(i => i.releaseYear),
            SortOption.RarityHigh => q.OrderByDescending(i => i.rarity),
            SortOption.RarityLow => q.OrderBy(i => i.rarity),
            SortOption.AcquiredNew => q, // 入手データと繋いでから（仮）
            SortOption.AcquiredOld => q, // 同上
            _ => q,
        };

        Rebuild(q);
    }
}