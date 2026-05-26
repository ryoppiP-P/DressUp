using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CategoryTabs : MonoBehaviour {
    [SerializeField] private DressupGrid grid;

    [System.Serializable]
    public class Tab {
        public Button button;
        public bool isAll;            // ALLタブだけチェック
        public CategoryType category; // ALL以外はここに対応カテゴリを入れる
    }

    [SerializeField] private List<Tab> tabs;
    [SerializeField] private Color selectedColor = new Color(1f, 0.48f, 0.64f);
    [SerializeField] private Color normalColor = Color.white;

    void Start() {
        foreach (var t in tabs) {
            var captured = t;
            captured.button.onClick.AddListener(() => OnTabClicked(captured));
        }

        // 初期状態：ALLタブを選択状態にする
        var allTab = tabs.Find(t => t.isAll);
        if (allTab != null) OnTabClicked(allTab);
    }

    void OnTabClicked(Tab tab) {
        // 1. グリッドを切り替える
        if (tab.isAll)
            grid.ShowCategory(null);
        else
            grid.ShowCategory(tab.category);

        // 2. タブの見た目を更新
        UpdateTabVisual(tab);
    }

    void UpdateTabVisual(Tab selected) {
        foreach (var t in tabs) {
            var img = t.button.GetComponent<Image>();
            img.color = (t == selected) ? selectedColor : normalColor;
        }
    }
}
