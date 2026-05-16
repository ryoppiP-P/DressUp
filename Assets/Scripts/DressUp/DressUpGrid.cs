using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DressupGrid : MonoBehaviour {
    [SerializeField] private Character character;
    [SerializeField] private Transform gridContent;     // Grid Layout Group の Content
    [SerializeField] private ItemButton itemButtonPrefab;
    [SerializeField] private List<DressUpItem> items;   // 表示したいアイテムを並べる

    private readonly List<ItemButton> _spawned = new();

    void Start() {
        ShowCategory(null); // 最初は ALL を表示
    }

    public void ShowCategory(CategoryType? category) {
        // 既存のボタンを片付ける
        foreach (var b in _spawned) Destroy(b.gameObject);
        _spawned.Clear();

        // 表示対象を絞り込む
        var filtered = (category == null)
            ? items
            : items.Where(i => i.category == category.Value).ToList();

        // 並べる
        foreach (var item in filtered) {
            var btn = Instantiate(itemButtonPrefab, gridContent);
            btn.Setup(item, character);
            _spawned.Add(btn);
        }
    }
}
