using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SubCategoryTabs : MonoBehaviour {
    [SerializeField] private DressupGrid grid;
    [SerializeField] private List<SubCategoryButton> buttons; // 下段ボタンを全種類登録
    [SerializeField, Range(0f, 1f)] private float selectedAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float normalAlpha = 0.6f;

    private CategoryGroup _currentGroup;
    private readonly List<SubCategoryButton> _visible = new();

    void Awake() {
        // 各ボタンのクリックを一度だけ登録
        foreach (var b in buttons) {
            var captured = b;
            captured.Button.onClick.AddListener(() => OnClicked(captured));
        }
    }

    // 上段から呼ばれる：この大分類に属するボタンだけ表示
    public void Build(CategoryGroup group) {
        Debug.Log($"Build called: {group} / buttons.Count = {buttons.Count}");
        _currentGroup = group;
        _visible.Clear();

        foreach (var b in buttons) {
            bool show = b.groups.Contains(group);
            Debug.Log($"  {b.name}: groups=[{string.Join(",", b.groups)}] show={show}");
            b.gameObject.SetActive(show);
            if (show) _visible.Add(b);
        }

        // 先頭の表示ボタンを初期選択
        if (_visible.Count > 0) OnClicked(_visible[0]);
    }

    void OnClicked(SubCategoryButton clicked) {
        // 選択中は不透明、それ以外はアルファを下げる
        foreach (var b in _visible)
            b.SetAlpha(b == clicked ? selectedAlpha : normalAlpha);

        grid.Show(_currentGroup, clicked.kind, clicked.category);
    }
}
