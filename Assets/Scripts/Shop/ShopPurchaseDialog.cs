//==============================================================================
//  File   : ShopPurchaseDialog.cs
//  Brief  : ショップの購入確認ダイアログ(アイテム写真・説明・はい/いいえ)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//  「はい」で SaveManager.TrySpendCurrency を呼んで購入を確定する。
//  所持・在庫の仕組みは無いため、購入は何度でも可能(通貨を消費するのみ)。
//==============================================================================
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPurchaseDialog : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("アイテム表示")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image priceCurrencyIcon;
    [SerializeField] private TMP_Text priceText;

    [Header("価格表示アイコン(どんぐり/はちみつ)")]
    [SerializeField] private Sprite nutSprite;
    [SerializeField] private Sprite honeySprite;

    [Header("ボタン")]
    [SerializeField] private Button yesButton;  // 購入する
    [SerializeField] private Button noButton;   // やめる

    private ShopListing _listing;

    void Awake() {
        if (yesButton) yesButton.onClick.AddListener(OnClickYes);
        if (noButton) noButton.onClick.AddListener(OnClickNo);
    }

    /// <summary>購入確認ダイアログを開く</summary>
    public void Open(ShopListing listing) {
        if (listing == null) return;
        _listing = listing;

        var item = listing.item;
        if (itemIcon) { itemIcon.sprite = item != null ? item.icon : null; itemIcon.enabled = item != null && item.icon != null; }
        if (itemNameText) itemNameText.text = item != null ? item.itemName : "";
        if (descriptionText) descriptionText.text = item != null ? item.description : "";
        if (priceText) priceText.text = listing.price.ToString();
        if (priceCurrencyIcon) priceCurrencyIcon.sprite = listing.currencyType == CurrencyType.Nut ? nutSprite : honeySprite;

        if (panelRoot) panelRoot.SetActive(true);
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
        _listing = null;
    }

    // 購入を確定する。所持数が足りない場合は何もしない(TrySpendCurrencyがfalseを返す)
    private void OnClickYes() {
        if (_listing == null || SaveManager.Instance == null) { Close(); return; }

        bool bought = SaveManager.Instance.TrySpendCurrency(_listing.currencyType, _listing.price);
        if (!bought) Debug.Log($"[Shop] 通貨が足りません: {(_listing.item != null ? _listing.item.itemName : _listing.name)}");

        Close();
    }

    private void OnClickNo() {
        Close();
    }
}
