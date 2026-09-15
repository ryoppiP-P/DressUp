//==============================================================================
//  File   : ShopPanel.cs
//  Brief  : ショップ画面のルート制御(グリッド表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//  MissionPanel / MenuPanel と同じ構成方針(Open/Close + SetActive切り替え)。
//  どんぐり/はちみつのタブは廃止し、両方のアイテムを1つのグリッドにまとめて表示する
//  (通貨種別はスロットごとの価格表示アイコンで見分ける)。
//  戻るボタンは廃止し、ボトムバーの自アイコンをもう一度押すと閉じるトグル方式にした。
//==============================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("グリッド")]
    [SerializeField] private ShopSlot slotPrefab;
    [SerializeField] private Transform contentParent; // ScrollView の Content

    [Header("データソース")]
    [SerializeField] private ShopDatabase shopDatabase; // 現状は空(アイテムは別途作成中)

    [Header("購入確認ダイアログ")]
    [SerializeField] private ShopPurchaseDialog purchaseDialog;

    private readonly List<ShopSlot> _spawned = new();

    // GameManagerの汎用TogglePanel/SetActiveなど、Open()を経由せずこのGameObjectが
    // 直接アクティブ化されるルートでもグリッドが必ず作り直されるようにする。
    // 排他制御(他パネルを閉じる)もOnEnable/OnDisableで行う(SetActiveされた経路に依らず必ず効く)。
    void OnEnable() {
        BottomPanelCoordinator.NotifyOpened(Close);
        Rebuild();
    }

    void OnDisable() {
        BottomPanelCoordinator.NotifyClosed(Close);
    }

    /// <summary>ショップ画面を開く</summary>
    public void Open() {
        if (panelRoot) panelRoot.SetActive(true);
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    /// <summary>開いていれば閉じる、閉じていれば開く(ボトムバーのアイコンから呼ぶ)</summary>
    public void ToggleOpen() {
        if (panelRoot != null && panelRoot.activeSelf) Close();
        else Open();
    }

    private void Rebuild() {
        foreach (var slot in _spawned) Destroy(slot.gameObject);
        _spawned.Clear();

        if (shopDatabase == null || shopDatabase.listings == null) return; // データ未実装(枠のみ)

        foreach (var listing in shopDatabase.listings) {
            if (listing == null) continue;
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
