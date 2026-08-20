//==============================================================================
//  File   : GachaPanel.cs
//  Brief  : ガチャ画面のルート制御(街装飾/服タブ切り替え・抽選・結果表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//  確率仕様(設計書より):
//    レアリティ確率  N:78% / R:20% / SR:2%
//    1個あたりの確率 = レアリティ確率 ÷ そのレアリティの対象アイテム数
//    10連ガチャは R以上を1個確定で保証する(9回通常抽選 + 保証済みなら10回目も通常抽選)
//  抽選結果は保存しない(ポップアップで見せるだけ)。
//==============================================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaPanel : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("カテゴリタブ(街装飾/服)")]
    [SerializeField] private Button decorationTab;
    [SerializeField] private Button clothesTab;
    [SerializeField] private Image decorationTabBg;
    [SerializeField] private Image clothesTabBg;
    [SerializeField] private Color tabSelectedColor = new Color(0.95f, 0.75f, 0.35f, 1f);
    [SerializeField] private Color tabUnselectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    [Header("イラスト表示部(ダミーの間はカテゴリ名だけ表示)")]
    [SerializeField] private TMP_Text illustrationLabel;

    [Header("ガチャボタン")]
    [SerializeField] private Button singlePullButton;
    [SerializeField] private TMP_Text singlePullCostText;
    [SerializeField] private Button tenPullButton;
    [SerializeField] private TMP_Text tenPullCostText;
    [SerializeField] private int singlePullCost = 100;  // 消費はちみつ(1回)
    [SerializeField] private int tenPullCost = 1000;     // 消費はちみつ(10回)

    [Header("データソース")]
    [SerializeField] private GachaDatabase gachaDatabase;

    [Header("結果ポップアップ")]
    [SerializeField] private GachaResultPopup resultPopup;

    [Header("戻るボタン")]
    [SerializeField] private Button backButton;

    // レアリティ別の抽選確率(%)
    private const float SRPercent = 2f;
    private const float RPercent = 20f;
    private const float NPercent = 78f;

    private GachaCategory _current = GachaCategory.Decoration;

    void Start() {
        if (decorationTab) decorationTab.onClick.AddListener(() => ShowCategory(GachaCategory.Decoration));
        if (clothesTab) clothesTab.onClick.AddListener(() => ShowCategory(GachaCategory.Clothes));
        if (singlePullButton) singlePullButton.onClick.AddListener(() => TryPull(1));
        if (tenPullButton) tenPullButton.onClick.AddListener(() => TryPull(10));
        if (backButton) backButton.onClick.AddListener(Close);

        if (singlePullCostText) singlePullCostText.text = singlePullCost.ToString();
        if (tenPullCostText) tenPullCostText.text = tenPullCost.ToString();
    }

    /// <summary>ガチャ画面を開く(街装飾タブから開始)</summary>
    public void Open() {
        if (panelRoot) panelRoot.SetActive(true);
        ShowCategory(GachaCategory.Decoration);
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    /// <summary>カテゴリ(街装飾/服)を切り替える</summary>
    public void ShowCategory(GachaCategory category) {
        _current = category;
        if (decorationTabBg) decorationTabBg.color = category == GachaCategory.Decoration ? tabSelectedColor : tabUnselectedColor;
        if (clothesTabBg) clothesTabBg.color = category == GachaCategory.Clothes ? tabSelectedColor : tabUnselectedColor;
        if (illustrationLabel) illustrationLabel.text = category == GachaCategory.Decoration ? "街装飾ガチャ" : "服ガチャ";
    }

    //--------------------------------------------------------------
    // 抽選の実行
    //--------------------------------------------------------------
    private void TryPull(int count) {
        if (SaveManager.Instance == null) return;

        // 排出するものが1つも登録されていないカテゴリでは引かせない。
        // (この判定が無いと、先にはちみつを払ってから DrawOne が null を返すので、
        //  何も出ないのに通貨だけ減る)
        if (CountCandidates() == 0) {
            Debug.Log($"[Gacha] {_current} に排出アイテムが登録されていません");
            return;
        }

        int cost = count == 1 ? singlePullCost : tenPullCost;
        if (!SaveManager.Instance.TrySpendCurrency(CurrencyType.Honey, cost)) {
            Debug.Log("[Gacha] はちみつが足りません");
            return;
        }

        List<GachaEntry> results = count == 1 ? DrawSingle() : DrawTen();

        // 引いたアイテムを所持アイテムとして記録する(アイテム一覧画面に出るようになる)
        foreach (var entry in results) {
            if (entry != null && entry.item != null)
                SaveManager.Instance.AddOwnedItem(entry.item.itemId);
        }

        if (resultPopup) resultPopup.Show(results);
    }

    // 1回ガチャ(通常抽選のみ)
    private List<GachaEntry> DrawSingle() {
        var entry = DrawOne(forceRareOrAbove: false);
        return entry != null ? new List<GachaEntry> { entry } : new List<GachaEntry>();
    }

    // 10回ガチャ(R以上1個確定)
    private List<GachaEntry> DrawTen() {
        var results = new List<GachaEntry>();
        bool hasRareOrAbove = false;

        for (int i = 0; i < 9; i++) {
            var entry = DrawOne(forceRareOrAbove: false);
            if (entry == null) continue;
            results.Add(entry);
            if (entry.item.rarity != Rarity.Normal) hasRareOrAbove = true;
        }

        // まだR以上が出ていなければ、最後の1回はR以上を確定で引く
        var last = DrawOne(forceRareOrAbove: !hasRareOrAbove);
        if (last != null) results.Add(last);

        return results;
    }

    // 今のカテゴリに排出候補が何件あるか
    private int CountCandidates() {
        if (gachaDatabase == null || gachaDatabase.entries == null) return 0;
        return gachaDatabase.entries.Count(e => e != null && e.item != null && e.category == _current);
    }

    // 現在のカテゴリから1個抽選する
    private GachaEntry DrawOne(bool forceRareOrAbove) {
        var candidatesInCategory = gachaDatabase != null && gachaDatabase.entries != null
            ? gachaDatabase.entries.Where(e => e != null && e.item != null && e.category == _current).ToList()
            : new List<GachaEntry>();
        if (candidatesInCategory.Count == 0) return null;

        Rarity rarity = DrawRarity(forceRareOrAbove);
        var sameRarity = candidatesInCategory.Where(e => e.item.rarity == rarity).ToList();
        var pool = sameRarity.Count > 0 ? sameRarity : candidatesInCategory; // 該当レアリティが無い場合はカテゴリ全体から救済

        return pool[Random.Range(0, pool.Count)];
    }

    // レアリティ抽選(N:78% / R:20% / SR:2%)。forceRareOrAboveの時はR:SRの比率(20:2)だけで抽選する
    private Rarity DrawRarity(bool forceRareOrAbove) {
        if (forceRareOrAbove) {
            float srShare = SRPercent / (RPercent + SRPercent) * 100f;
            return Random.Range(0f, 100f) < srShare ? Rarity.SuperRare : Rarity.Rare;
        }

        float roll = Random.Range(0f, 100f);
        if (roll < SRPercent) return Rarity.SuperRare;
        if (roll < SRPercent + RPercent) return Rarity.Rare;
        return Rarity.Normal;
    }
}
