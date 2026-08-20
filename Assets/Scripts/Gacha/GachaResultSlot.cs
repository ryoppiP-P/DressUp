//==============================================================================
//  File   : GachaResultSlot.cs
//  Brief  : ガチャ結果ポップアップの1枠分の表示(アイコン・名前・レアリティ)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//  名前は showItemName で出し分ける。今は OFF にして、そのぶんアイテムの絵を
//  枠いっぱいに大きく見せている。名前を戻したい時は Inspector で ON にすれば、
//  下に名前の場所を空けたレイアウトへ自動で戻る(NameText は消していない)。
//==============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaResultSlot : MonoBehaviour {
    [Header("表示")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image rarityBadge;
    [SerializeField] private RarityIconTable rarityTable; // DressUp側と共通のレアリティアイコン表

    [Header("名前の表示")]
    [Tooltip("OFFにすると名前を隠し、そのぶんアイテムの絵を枠いっぱいに出す")]
    [SerializeField] private bool showItemName = false;

    [Header("アイコンの配置")]
    [Tooltip("枠の内側に取る余白")]
    [SerializeField] private float iconMargin = 12f;
    [Tooltip("名前を出す時に下へ空ける高さ")]
    [SerializeField] private float nameAreaHeight = 60f;

    /// <summary>抽選結果の1件分をセットする</summary>
    public void Setup(GachaEntry entry) {
        if (entry == null || entry.item == null) return;
        var item = entry.item;

        if (iconImage) { iconImage.sprite = item.icon; iconImage.enabled = item.icon != null; }

        // 名前は隠していても中身は入れておく(表示を戻した時にそのまま出るように)
        if (nameText) {
            nameText.text = item.itemName;
            nameText.gameObject.SetActive(showItemName);
        }
        ApplyIconLayout();

        if (rarityBadge) {
            var sprite = rarityTable != null ? rarityTable.GetIcon(item.rarity) : null;
            rarityBadge.sprite = sprite;
            rarityBadge.enabled = sprite != null;
        }
    }

    // 名前を出すかどうかでアイコンの大きさを変える
    private void ApplyIconLayout() {
        if (iconImage == null) return;

        var rect = iconImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(iconMargin, iconMargin + (showItemName ? nameAreaHeight : 0f));
        rect.offsetMax = new Vector2(-iconMargin, -iconMargin);
    }

#if UNITY_EDITOR
    // Inspector で切り替えた時にシーン上でもすぐ反映されるように。
    // OnValidate の中で RectTransform を書き換えると Unity に怒られる
    // (SendMessage cannot be called during OnValidate)ので、1フレーム遅らせる。
    private void OnValidate() {
        UnityEditor.EditorApplication.delayCall += () => {
            if (this == null) return;

            if (nameText) nameText.gameObject.SetActive(showItemName);
            ApplyIconLayout();
        };
    }
#endif
}
