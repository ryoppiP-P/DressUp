//==============================================================================
//  File   : ShopSlot.cs
//  Brief  : ショップグリッドの1マス分の表示(アイコン・名前・価格)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//==============================================================================
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour {
    [Header("表示")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image priceCurrencyIcon;
    [SerializeField] private TMP_Text priceText;

    [Header("価格表示アイコン(どんぐり/はちみつ)")]
    [SerializeField] private Sprite nutSprite;
    [SerializeField] private Sprite honeySprite;

    [Header("ボタン")]
    [SerializeField] private Button button;

    private ShopListing _listing;
    private Action<ShopListing> _onClick;

    /// <summary>出品情報をセットし、タップ時に呼ぶコールバックを登録する</summary>
    public void Setup(ShopListing listing, Action<ShopListing> onClick) {
        _listing = listing;
        _onClick = onClick;

        var item = listing.item;
        if (iconImage) { iconImage.sprite = item != null ? item.icon : null; iconImage.enabled = item != null && item.icon != null; }
        if (nameText) nameText.text = item != null ? item.itemName : "";
        if (priceText) priceText.text = listing.price.ToString();
        if (priceCurrencyIcon) priceCurrencyIcon.sprite = listing.currencyType == CurrencyType.Nut ? nutSprite : honeySprite;

        if (button) {
            button.onClick.RemoveAllListeners(); // 使い回し時の二重登録防止
            button.onClick.AddListener(() => _onClick?.Invoke(_listing));
        }
    }
}
