//==============================================================================
//  File   : GachaResultSlot.cs
//  Brief  : ガチャ結果ポップアップの1枠分の表示(アイコン・名前・レアリティ)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
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

    /// <summary>抽選結果の1件分をセットする</summary>
    public void Setup(GachaEntry entry) {
        if (entry == null || entry.item == null) return;
        var item = entry.item;

        if (iconImage) { iconImage.sprite = item.icon; iconImage.enabled = item.icon != null; }
        if (nameText) nameText.text = item.itemName;

        if (rarityBadge) {
            var sprite = rarityTable != null ? rarityTable.GetIcon(item.rarity) : null;
            rarityBadge.sprite = sprite;
            rarityBadge.enabled = sprite != null;
        }
    }
}
