//==============================================================================
//  File   : ItemListPanel.cs
//  Brief  : アイテム一覧画面(タブ切り替え + グリッド表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//  タブ構成: ファッション / 装飾 / その他(時短アイテム)
//  ・ファッションタブ … 既存の DressUpItem / ItemDatabase をそのまま表示する
//  ・装飾 / その他タブ … 専用データがまだ無いため、枠(タブ切り替えと空表示)だけ用意。
//                        専用の ScriptableObject ができたら GetItemsForTab に差し込む。
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
    [SerializeField] private ItemDatabase itemDatabase; // ファッションタブで使用(DressUp と共通)

    [Header("未実装タブの案内表示")]
    [SerializeField] private GameObject comingSoonLabel; // 装飾/その他タブ選択時に表示

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

        var items = GetItemsForTab(_current);

        // データソース未実装のタブは案内テキストだけ出して終わる
        if (comingSoonLabel) comingSoonLabel.SetActive(items == null);
        if (items == null) return;

        foreach (var item in items) {
            if (item == null) continue; // 欠損参照はスキップ(クラッシュ防止)
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(item);
            _spawned.Add(slot);
        }
    }

    // タブごとの表示アイテムを取得する。null を返したタブは「未実装(枠のみ)」扱い。
    private IEnumerable<DressUpItem> GetItemsForTab(ItemListTab tab) {
        switch (tab) {
            case ItemListTab.Fashion:
                // 着せ替えアイテムは全カテゴリ(髪/トップス/アクセサリー/目・口 等)をまとめて表示する
                if (itemDatabase == null || itemDatabase.allItems == null) return Enumerable.Empty<DressUpItem>();
                return itemDatabase.allItems.Where(i => i != null);

            case ItemListTab.Accessory:
                // TODO: 装飾アイテム用の ScriptableObject / データベースができたらここに差し込む
                return null;

            case ItemListTab.Other:
                // TODO: その他(時短アイテム)用の ScriptableObject / データベースができたらここに差し込む
                return null;

            default:
                return Enumerable.Empty<DressUpItem>();
        }
    }

    private void OnClickBack() {
        if (menuPanel) menuPanel.ShowMain();
    }
}
