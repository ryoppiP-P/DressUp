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
using System.Collections;
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
    [SerializeField] private RectTransform illustrationArea; // 抽選演出でバウンドさせる対象

    [Header("抽選演出(ボタンを押してから結果が出るまでの「ガコン！」)")]
    [SerializeField] private Image pullFlashImage;    // 画面全体に一瞬光らせる白フラッシュ(初期alpha0)
    [SerializeField] private string pullingMessage = "……";
    [SerializeField] private float pullAnticipationSeconds = 0.5f;

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

    [Header("通貨が足りない時に出すトースト(任意)")]
    [SerializeField] private SimpleMessagePopup insufficientFundsPopup;
    [SerializeField] private string insufficientFundsMessage = "お金が足りないよ!";

    // レアリティ別の抽選確率(%)
    private const float SRPercent = 2f;
    private const float RPercent = 20f;
    private const float NPercent = 78f;

    private GachaCategory _current = GachaCategory.Decoration;

    void Start() {
        if (decorationTab) decorationTab.onClick.AddListener(() => ShowCategory(GachaCategory.Decoration));
        if (clothesTab) clothesTab.onClick.AddListener(() => ShowCategory(GachaCategory.Clothes));
        if (singlePullButton) singlePullButton.onClick.AddListener(() => StartCoroutine(PullRoutine(1)));
        if (tenPullButton) tenPullButton.onClick.AddListener(() => StartCoroutine(PullRoutine(10)));
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
    private bool _pulling; // 演出中の連打防止

    private IEnumerator PullRoutine(int count) {
        if (_pulling) yield break;
        if (SaveManager.Instance == null) yield break;

        // 排出するものが1つも登録されていないカテゴリでは引かせない。
        // (この判定が無いと、先にはちみつを払ってから DrawOne が null を返すので、
        //  何も出ないのに通貨だけ減る)
        if (CountCandidates() == 0) {
            Debug.Log($"[Gacha] {_current} に排出アイテムが登録されていません");
            yield break;
        }

        int cost = count == 1 ? singlePullCost : tenPullCost;
        if (!SaveManager.Instance.TrySpendCurrency(CurrencyType.Honey, cost)) {
            Debug.Log("[Gacha] はちみつが足りません");
            if (insufficientFundsPopup != null) insufficientFundsPopup.Show(insufficientFundsMessage);
            yield break;
        }

        List<GachaEntry> results = count == 1 ? DrawSingle() : DrawTen();

        // 引いたアイテムを所持アイテムとして記録する(アイテム一覧画面に出るようになる)
        foreach (var entry in results) {
            if (entry != null && entry.item != null)
                SaveManager.Instance.AddOwnedItem(entry.item.itemId);
        }

        // ミッション「ガチャをしよう」。引いた回数ぶん数える(1回ガチャ=1、10連=10)
        if (MissionManager.Instance != null)
            MissionManager.Instance.Report(MissionType.Gacha, count);

        // 結果を見せる前に「ガコン！」の一瞬(連打防止も兼ねてボタンを封じる)
        _pulling = true;
        SetPullButtonsInteractable(false);
        yield return PullAnticipationRoutine();
        SetPullButtonsInteractable(true);
        _pulling = false;

        if (resultPopup) resultPopup.Show(results);
    }

    // ボタンを押してから結果が出るまでの「溜め」演出。
    // イラスト表示部をバウンドさせつつ、最後に白いフラッシュで一瞬視界を切る。
    private IEnumerator PullAnticipationRoutine() {
        string originalText = illustrationLabel != null ? illustrationLabel.text : null;
        if (illustrationLabel != null) illustrationLabel.text = pullingMessage;

        if (illustrationArea != null) yield return BounceRoutine(illustrationArea, pullAnticipationSeconds * 0.7f, 1.3f);
        else yield return new WaitForSeconds(pullAnticipationSeconds * 0.7f);

        if (pullFlashImage != null) yield return FlashRoutine(pullFlashImage, pullAnticipationSeconds * 0.3f);

        if (illustrationLabel != null && originalText != null) illustrationLabel.text = originalText;
    }

    // back-ease-out で弾むようにスケールを揺らす(押した瞬間の「グン！」を出す)
    private static IEnumerator BounceRoutine(RectTransform rt, float duration, float overshoot) {
        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float back = p - 1f;
            float scale = back * back * ((overshoot + 1f) * back + overshoot) + 1f;
            rt.localScale = Vector3.one * scale;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    // 画面を白く一瞬光らせてから消す
    private static IEnumerator FlashRoutine(Image image, float duration) {
        image.gameObject.SetActive(true);
        var c = image.color;

        float half = duration * 0.5f;
        float t = 0f;
        while (t < half) {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / half);
            image.color = c;
            yield return null;
        }
        t = 0f;
        while (t < half) {
            t += Time.deltaTime;
            c.a = 1f - Mathf.Clamp01(t / half);
            image.color = c;
            yield return null;
        }

        c.a = 0f;
        image.color = c;
        image.gameObject.SetActive(false);
    }

    private void SetPullButtonsInteractable(bool on) {
        if (singlePullButton) singlePullButton.interactable = on;
        if (tenPullButton) tenPullButton.interactable = on;
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
