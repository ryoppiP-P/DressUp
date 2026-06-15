using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SidePanelTabs : MonoBehaviour {
    [System.Serializable]
    public class Entry {
        public Button button;
        public Image buttonImage;   // 色を差し替える対象
        public Sprite brightSprite; // 選択中（明るい画像）
        public Sprite darkSprite;   // 非選択（暗い画像）
        public GameObject panel;    // 中身
    }

    [SerializeField] private RectTransform rightBar; // 動かすバー本体
    [SerializeField] private List<Entry> entries;
    [SerializeField] private float openX = -259f;    // 開いている時の PosX
    [SerializeField] private float closeX = -5f;      // 閉じている時の PosX

    private Entry _open = null;

    void Start() {
        foreach (var e in entries) {
            var captured = e;
            captured.button.onClick.AddListener(() => Toggle(captured));
        }
        ApplyState(); // 初期：閉じた状態
    }

    void Toggle(Entry tapped) {
        // 同じボタンをもう一度押したら閉じる
        _open = (_open == tapped) ? null : tapped;
        ApplyState();
    }

    void ApplyState() {
        // バーの位置
        float targetX = (_open != null) ? openX : closeX;
        var pos = rightBar.anchoredPosition;
        pos.x = targetX;
        rightBar.anchoredPosition = pos;

        // ボタン画像の差し替え
        foreach (var e in entries) {
            bool isSelected = (e == _open);
            e.buttonImage.sprite = isSelected ? e.brightSprite : e.darkSprite;
            if (e.panel) e.panel.SetActive(isSelected); // 選択中だけ表示
        }
    }
}
