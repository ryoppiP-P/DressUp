//==============================================================================
//  File   : ShopPanel.cs
//  Brief  : ショップ画面のルート制御(どんぐり/はちみつタブ切り替え・グリッド表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//  MissionPanel / MenuPanel と同じ構成方針(Open/Close + SetActive切り替え)。
//  shopDatabase は現時点で空の想定(アイテムは別途作成中のため、枠のみ用意)。
//==============================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("タブ(どんぐり/はちみつ)")]
    [SerializeField] private Button nutTab;
    [SerializeField] private Button honeyTab;
    [SerializeField] private Image nutTabBg;
    [SerializeField] private Image honeyTabBg;
    [SerializeField] private Color tabSelectedColor = new Color(0.95f, 0.75f, 0.35f, 1f);
    [SerializeField] private Color tabUnselectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    [Header("グリッド")]
    [SerializeField] private ShopSlot slotPrefab;
    [SerializeField] private Transform contentParent; // ScrollView の Content

    [Header("データソース")]
    [SerializeField] private ShopDatabase shopDatabase; // 現状は空(アイテムは別途作成中)

    [Header("購入確認ダイアログ")]
    [SerializeField] private ShopPurchaseDialog purchaseDialog;

    [Header("戻るボタン")]
    [SerializeField] private Button backButton;

    private readonly List<ShopSlot> _spawned = new();
    private CurrencyType _current = CurrencyType.Nut;

    void Start() {
        if (nutTab) nutTab.onClick.AddListener(() => ShowTab(CurrencyType.Nut));
        if (honeyTab) honeyTab.onClick.AddListener(() => ShowTab(CurrencyType.Honey));
        if (backButton) backButton.onClick.AddListener(Close);
    }

    // GameManagerの汎用TogglePanel/SetActiveなど、Open()を経由せずこのGameObjectが
    // 直接アクティブ化されるルートでもグリッドが必ず作り直されるようにする
    void OnEnable() {
        ShowTab(_current);
    }

    /// <summary>ショップ画面を開く(どんぐりショップから開始)</summary>
    public void Open() {
        if (panelRoot) panelRoot.SetActive(true);
        ShowTab(CurrencyType.Nut);
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    /// <summary>タブを切り替えてグリッドを作り直す</summary>
    public void ShowTab(CurrencyType type) {
        _current = type;
        if (nutTabBg) nutTabBg.color = type == CurrencyType.Nut ? tabSelectedColor : tabUnselectedColor;
        if (honeyTabBg) honeyTabBg.color = type == CurrencyType.Honey ? tabSelectedColor : tabUnselectedColor;
        Rebuild();
    }

    private void Rebuild() {
        foreach (var slot in _spawned) Destroy(slot.gameObject);
        _spawned.Clear();

        if (shopDatabase == null || shopDatabase.listings == null) return; // データ未実装(枠のみ)

        foreach (var listing in shopDatabase.listings) {
            if (listing == null || listing.currencyType != _current) continue; // タブに合う通貨種別のみ
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(listing, OnClickItem);
            _spawned.Add(slot);
        }

        // パネルを開いた直後(非アクティブ→アクティブの1フレーム目)はレイアウトが未確定で
        // GridLayoutGroup/ContentSizeFitterの反映が1フレーム遅れることがあるため、即時に確定させる
        Canvas.ForceUpdateCanvases();
        if (contentParent is RectTransform contentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    // アイテムタップ → 購入確認ダイアログを開く
    private void OnClickItem(ShopListing listing) {
        if (purchaseDialog) purchaseDialog.Open(listing);
    }
}
