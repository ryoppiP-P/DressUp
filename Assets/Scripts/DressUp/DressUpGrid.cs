using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DressupGrid : MonoBehaviour {
    [SerializeField] private Transform gridContent;     // Grid Layout Group の Content
    [SerializeField] private ItemButton itemButtonPrefab;
    [SerializeField] private ItemDatabase database;     // ScriptableObject で作ったアイテムデータベース

    private readonly List<ItemButton> _spawned = new();

    // 表示条件は「タブ」「名前検索」「絞り込みパネル」の3つを別々に覚えておき、
    // 並べる時に全部を重ねて適用する。
    // ・対象キャラが後から決まった時に同じ条件で並べ直せる
    //   (これが無いと、着せ替え画面を開いた直後だけタブの選択と中身がズレる)
    // ・フィルターを適用してもタブの絞り込みが生きたままになる
    //   (以前はフィルターがタブごと上書きしていたので、
    //    「着用中」タブなのに着ていないものまで並んでいた)
    private bool _hasTab;
    private CategoryGroup _group;
    private FilterKind _kind = FilterKind.All;
    private CategoryType _category;
    private string _searchKeyword = "";
    private FilterCondition _condition;

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
        Refresh();
    }

    /// <summary>今の絞り込み条件のまま並べ直す（着脱した後など）</summary>
    public void Refresh() {
        Rebuild(BuildQuery());
    }

    // タブ → 名前検索 → 絞り込みパネル の順に重ねがけする
    private IEnumerable<DressUpItem> BuildQuery() {
        IEnumerable<DressUpItem> q = AllItems;

        // 1. タブ（大分類 / 小分類 / 着用中）
        if (_hasTab) {
            var groupCats = CategoryMap.GetCategories(_group);
            switch (_kind) {
                case FilterKind.Equipped:
                    q = q.Where(i => groupCats.Contains(i.category) && IsEquipped(i));
                    break;
                case FilterKind.Category:
                    q = q.Where(i => MatchesCategory(i, _category));
                    break;
                default:
                    q = q.Where(i => groupCats.Contains(i.category));
                    break;
            }
        }

        // 2. 検索欄の名前
        if (!string.IsNullOrEmpty(_searchKeyword))
            q = q.Where(i => i.itemName.Contains(_searchKeyword));

        // 3. 絞り込みパネルの条件
        var cond = _condition;
        if (cond != null) {
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
        }

        return q;
    }

    // 大分類 + 絞り込み種別 + (必要なら)小分類 で表示
    public void Show(CategoryGroup group, FilterKind kind, CategoryType category = default) {
        _hasTab = true;
        _group = group;
        _kind = kind;
        _category = category;

        Refresh();
    }

    // ヘアALLは「前髪と後ろ髪をまとめて見るタブ」なので、両方のアイテムを拾う
    private static bool MatchesCategory(DressUpItem item, CategoryType category) {
        if (item.category == category) return true;

        if (category == CategoryType.HairAll)
            return item.category == CategoryType.HairFront || item.category == CategoryType.HairBack;

        return false;
    }

    // 名前検索
    public void ShowByName(string keyword) {
        _searchKeyword = keyword;
        Refresh();
    }

    // 着ているかどうかは「今のキャラの見た目」で見る。
    // 保存状態(GetEquipState)を見ていた頃は、ContainsValue で全カテゴリを串刺しに探していたため
    // 別のカテゴリで着ているだけのアイテムまで着用中になっていた。
    private bool IsEquipped(DressUpItem item) {
        return character != null && character.IsWearing(item);
    }

    private void Rebuild(IEnumerable<DressUpItem> filtered) {
        foreach (var b in _spawned) {
            if (b == null) continue;

            // Destroy はフレーム終わりまで効かないので、先に消しておかないと
            // 並べ直した瞬間だけ古いボタンと新しいボタンが二重に並ぶ
            b.gameObject.SetActive(false);
            Destroy(b.gameObject);
        }
        _spawned.Clear();
        if (character == null) return;

        foreach (var item in filtered) {
            if (item == null) continue;   // null はスキップ（クラッシュ防止）
            var btn = Instantiate(itemButtonPrefab, gridContent);
            btn.Setup(item, character, Refresh);   // 着脱したら一覧を作り直す
            _spawned.Add(btn);
        }
    }

    /// <summary>絞り込みパネルの条件を差し替える。タブの絞り込みはそのまま残る。</summary>
    public void ApplyFilter(FilterCondition cond) {
        _condition = cond;
        Refresh();
    }
}