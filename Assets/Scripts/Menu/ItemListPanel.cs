//==============================================================================
//  File   : ItemListPanel.cs
//  Brief  : アイテム一覧画面(タブ切り替え + グリッド表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//  タブ構成: ファッション / 装飾 / その他
//  ・ファッション … 既存の ItemDatabase(DressUpItem)を全カテゴリまとめて表示
//  ・装飾         … TownCreateItem を入れた GameItemDatabase
//  ・その他       … OtherItem を入れた GameItemDatabase
//
//  showOnlyOwned が true の場合は SaveManager の所持リスト(itemId)で絞り込み、
//  設計書どおり「今まで集めたアイテム」だけを表示する。
//  false にすると所持に関係なく全件表示するので、UI の見た目確認に使える。
//==============================================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemListPanel : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("戻り先")]
    [SerializeField] private MenuPanel menuPanel;

    [Header("タブボタン")]
    [SerializeField] private Button fashionTab;
    [SerializeField] private Button accessoryTab;
    [SerializeField] private Button otherTab;

    [Header("グリッド")]
    [SerializeField] private ItemListSlot slotPrefab;
    [SerializeField] private Transform contentParent; // ScrollView の Content

    [Header("データソース")]
    [SerializeField] private ItemDatabase fashionDatabase;          // 着せ替え(DressUpItem)
    [Tooltip("着せ替え画面のDBには入れたくないファッションアイテム(ガチャ限定・ダミー等)")]
    [SerializeField] private GameItemDatabase fashionExtraDatabase; // 上記に合流させる追加分
    [SerializeField] private GameItemDatabase decorationDatabase;   // 街クリエイト(TownCreateItem)
    [SerializeField] private GameItemDatabase otherDatabase;        // その他(OtherItem)

    [Header("表示設定")]
    [Tooltip("ONで所持しているアイテムだけ表示する(設計書どおりの挙動)。OFFで全件表示")]
    [SerializeField] private bool showOnlyOwned = true;

    [Header("空のときの案内表示")]
    [SerializeField] private GameObject emptyLabel; // 表示するアイテムが1件も無いときに出す

    [Header("戻るボタン")]
    [SerializeField] private Button backButton;

    private readonly List<ItemListSlot> _spawned = new();
    private ItemListTab _current = ItemListTab.Fashion;

    void Start() {
        if (fashionTab) fashionTab.onClick.AddListener(() => ShowTab(ItemListTab.Fashion));
        if (accessoryTab) accessoryTab.onClick.AddListener(() => ShowTab(ItemListTab.Accessory));
        if (otherTab) otherTab.onClick.AddListener(() => ShowTab(ItemListTab.Other));
        if (backButton) backButton.onClick.AddListener(OnClickBack);
    }

    /// <summary>アイテム一覧画面を開く(常にファッションタブから開始)</summary>
    public void Open() {
        if (panelRoot) panelRoot.SetActive(true);
        ShowTab(ItemListTab.Fashion);
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    /// <summary>タブを切り替えてグリッドを作り直す</summary>
    public void ShowTab(ItemListTab tab) {
        _current = tab;
        Rebuild();
    }

    private void Rebuild() {
        foreach (var slot in _spawned) Destroy(slot.gameObject);
        _spawned.Clear();

        var items = GetItemsForTab(_current)
            .Where(i => i != null)          // 欠損参照はスキップ(クラッシュ防止)
            .Where(IsVisible)
            .ToList();

        // 1件も無ければ案内テキストだけ出して終わる
        if (emptyLabel) emptyLabel.SetActive(items.Count == 0);

        foreach (var item in items) {
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(item);
            _spawned.Add(slot);
        }
    }

    // 所持フィルタ。showOnlyOwned が OFF なら常に表示する
    private bool IsVisible(GameItem item) {
        if (!showOnlyOwned) return true;
        if (item.ownedByDefault) return true;              // 初期所持アイテムはセーブに無くても表示する
        if (SaveManager.Instance == null) return false;
        return SaveManager.Instance.IsItemOwned(item);
    }

    // タブごとの表示候補アイテムを取得する
    private IEnumerable<GameItem> GetItemsForTab(ItemListTab tab) {
        switch (tab) {
            case ItemListTab.Fashion: {
                // 着せ替えアイテムは全カテゴリ(髪/トップス/アクセサリー/目・口 等)をまとめて表示する。
                // 着せ替え画面のDBに入れていない追加分(ガチャ限定など)もここで合流させる。
                IEnumerable<GameItem> baseItems = (fashionDatabase != null && fashionDatabase.allItems != null)
                    ? fashionDatabase.allItems
                    : Enumerable.Empty<GameItem>();
                IEnumerable<GameItem> extraItems = (fashionExtraDatabase != null && fashionExtraDatabase.allItems != null)
                    ? fashionExtraDatabase.allItems
                    : Enumerable.Empty<GameItem>();
                return baseItems.Concat(extraItems);
            }

            case ItemListTab.Accessory:
                if (decorationDatabase == null || decorationDatabase.allItems == null) break;
                return decorationDatabase.allItems;

            case ItemListTab.Other:
                if (otherDatabase == null || otherDatabase.allItems == null) break;
                return otherDatabase.allItems;
        }
        return Enumerable.Empty<GameItem>();
    }

    private void OnClickBack() {
        if (menuPanel) menuPanel.ShowMain();
    }
}
