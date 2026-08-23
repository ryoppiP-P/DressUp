//==============================================================================
//  File   : FairyPotFocus.cs
//  Brief  : 鉢をタップした時にアップ画面を出す/戻す + 残り時間と短縮ボタンの置き場所
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/23
//------------------------------------------------------------------------------
//==============================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FairyPotFocus : MonoBehaviour {
    [Header("鉢のトグル(3つ)")]
    [SerializeField] private Toggle[] potToggles;

    [Header("各鉢の SeedTime(植わっているかの判定 / 残り時間の実物)")]
    [SerializeField] private SeedTime[] seedTimes;

    [Header("各鉢の「時間を短縮させる」ボタン(ItemUseManager が使う実物)")]
    [SerializeField] private GameObject[] reduceButtons;

    [Header("各鉢の芽(全景)。誕生後も名前を付けるまでは出しておく")]
    [SerializeField] private GameObject[] potSprouts;

    [Header("各鉢の「生まれた！」(全景。設計書どおり残り時間と同じ位置)")]
    [SerializeField] private GameObject[] bornLabels;

    [Header("誕生の演出(「生まれた！」の鉢をタップして開き直す)")]
    [SerializeField] private BirthPopup birthPopup;

    [Header("キーワードの「やっぱりやめる」(押したら全景へ戻す)")]
    [SerializeField] private Toggle cancelToggle;

    [Header("開くもの")]
    [SerializeField] private GameObject closeUpRoot;     // 鉢のアップ画面
    [SerializeField] private GameObject keywordPanel;    // どんな子に育ってほしい？
    [SerializeField] private GameObject bigSprout;       // アップ画面の芽

    [Header("アップ画面の「〇〇な子が生まれそうだ！」(拡大した時だけ出す)")]
    [SerializeField] private GameObject growMessageRoot;
    [SerializeField] private TMPro.TMP_Text growMessageText;

    [Header("アップ画面での置き場所(設計書: 拡大した鉢の下)")]
    [SerializeField] private RectTransform closeUpTimerSlot;
    [SerializeField] private RectTransform closeUpReduceSlot;

    [Header("アップ画面での残り時間の大きさ(全景に対する倍率)")]
    [SerializeField] private float closeUpTimerScale = 2.6f;

    [Header("戻るボタン")]
    [SerializeField] private Button backButton;

    private Layout[] _timerLayouts;
    private Layout[] _reduceLayouts;

    private int _focused = -1;
    private bool _started;      // Start より前のトグル操作(seedManager の復元)は無視する
    private bool _suppress;     // こちらから isOn を書き換えている間は反応しない
    private bool _born;         // 「生まれた！」を出している間

    /// <summary>今開いている鉢(0-2)。開いていなければ -1。</summary>
    public int FocusedSlot { get { return _focused; } }

    /// <summary>「生まれた！」を出している間は true</summary>
    public bool IsBornView { get { return _born; } }

    /// <summary>シーンに1つだけ置く想定。植える時にどの鉢かを知るために使う。</summary>
    public static FairyPotFocus Current { get; private set; }

    //--------------------------------------------------------------------------

    void Awake() {
        Current = this;

        int count = potToggles != null ? potToggles.Length : 0;
        _timerLayouts = new Layout[count];
        _reduceLayouts = new Layout[count];

        for (int i = 0; i < count; i++) {
            // 元の位置(全景での置き場所)を控えておく。戻る時にここへ返す。
            var timer = TimerOf(i);
            if (timer != null) _timerLayouts[i] = new Layout(timer);

            var reduce = ReduceOf(i);
            if (reduce != null) _reduceLayouts[i] = new Layout(reduce);
        }

        if (backButton != null) backButton.onClick.AddListener(Back);
    }

    void Start() {
        if (!_born) Close();   // 最初は全景
        StartCoroutine(Boot());
    }

    /// <summary>
    /// 鉢のタップに反応し始めるのは、全部の Start が終わってから。
    /// ・seedManager.Start() が「植わっている鉢のトグルを ON にする」復元をするので、
    ///   それをプレイヤーのタップと取り違えないため。
    /// ・トグルの通知は登録した順に呼ばれる。seedManager より後に登録しないと、
    ///   こちらが isOn を戻した後に seedManager が古い値で残り時間を消してしまう
    ///   (植わっている鉢を開いたのに時間が出ない、という形で出た)。
    /// </summary>
    private IEnumerator Boot() {
        yield return null;

        for (int i = 0; potToggles != null && i < potToggles.Length; i++) {
            if (potToggles[i] == null) continue;

            int index = i;   // クロージャ対策
            UnityAction<bool> handler = on => OnPotToggled(index, on);
            potToggles[i].onValueChanged.AddListener(handler);
        }

        // 「やっぱりやめる」は久保木さんの cancelManager がキーワードを片付けるので、
        // こちらはアップ画面を閉じて全景へ帰すところだけを足す。
        if (cancelToggle != null) cancelToggle.onValueChanged.AddListener(OnCancelToggled);

        _started = true;
        if (!_born) Close();
    }

    private void OnCancelToggled(bool on) {
        if (on && _started && !_born) Close();
    }

    void OnDestroy() {
        if (Current == this) Current = null;
    }

    //--------------------------------------------------------------------------
    // 鉢のタップ
    //--------------------------------------------------------------------------

    private void OnPotToggled(int index, bool on) {
        // seedManager は「植わっている鉢のトグルを ON にする」復元をするので、
        // Start が終わるまでの変化はプレイヤーのタップではない。
        if (!_started || _suppress || _born) return;

        // 見ている鉢をもう一度タップしたら閉じる
        if (_focused == index) {
            Close();
            return;
        }

        // 「生まれた！」が出ている鉢なら、誕生の画面を開き直す
        if (index == PendingBornSlot() && birthPopup != null && birthPopup.ShowPending()) return;

        Open(index);
    }

    /// <summary>鉢のアップを開く</summary>
    public void Open(int index) {
        if (potToggles == null || index < 0 || index >= potToggles.Length) return;

        _focused = index;
        bool planted = IsPlanted(index);

        // 残り時間の更新(seedManager.Update)はトグルが ON の間だけ動くので、
        // 見ている鉢は ON にしておく。誕生後は空の鉢なので同期だけでよい。
        SyncToggles(_born ? -1 : index);

        RestoreAll();

        if (closeUpRoot != null) closeUpRoot.SetActive(true);
        if (keywordPanel != null) keywordPanel.SetActive(!planted && !_born);   // 空の鉢だけキーワードを聞く
        if (bigSprout != null) bigSprout.SetActive(planted || _born);

        RefreshPots();
        ShowGrowMessage(planted && !_born ? index : -1);

        if (!planted || _born) return;

        // 設計書どおり、拡大した鉢の下に大きく並べ直す
        if (_timerLayouts[index] != null && closeUpTimerSlot != null)
            _timerLayouts[index].MoveTo(closeUpTimerSlot, closeUpTimerScale);

        if (_reduceLayouts[index] != null && closeUpReduceSlot != null) {
            _reduceLayouts[index].MoveTo(closeUpReduceSlot, 1f);
            reduceButtons[index].SetActive(true);
        }
    }

    /// <summary>今見ている鉢を開き直す(種を植えた直後など)</summary>
    public void Refresh() {
        if (_focused >= 0) Open(_focused);
    }

    /// <summary>全景に戻る</summary>
    public void Close() {
        // 誕生の演出を出したまま帰らない(妖精は名前待ちのまま鉢に残る)
        if (_born && birthPopup != null) birthPopup.Hide();

        _focused = -1;
        _born = false;

        RestoreAll();

        if (closeUpRoot != null) closeUpRoot.SetActive(false);
        if (keywordPanel != null) keywordPanel.SetActive(false);
        if (bigSprout != null) bigSprout.SetActive(false);

        SyncToggles(-1);
        RefreshPots();
        ShowGrowMessage(-1);
    }

    /// <summary>
    /// 「〇〇な子が生まれそうだ！」を出す(設計書 III-I)。
    /// 全景では出さないので、閉じる時は -1 を渡して消す。
    /// </summary>
    private void ShowGrowMessage(int slotIndex) {
        string message = slotIndex >= 0 ? FairyPersonalityMessage.ForSlot(slotIndex) : "";
        bool show = !string.IsNullOrEmpty(message);

        if (growMessageText != null && show) growMessageText.text = message;
        if (growMessageRoot != null) growMessageRoot.SetActive(show);
    }

    private void Back() {
        Close();
    }

    //--------------------------------------------------------------------------
    // 誕生
    //--------------------------------------------------------------------------

    /// <summary>
    /// 「生まれた！」の画面を出す(BirthPopup から呼ばれる)。
    /// 生まれた後のスロットは空なので、キーワードも残り時間も出さずに芽だけ見せる。
    /// 「戻る」で全景へ帰れるが、その時は鉢の下に小さく「生まれた！」が残る。
    /// </summary>
    public void OpenForBirth(int slotIndex) {
        if (potToggles == null || potToggles.Length == 0) return;

        if (slotIndex < 0 || slotIndex >= potToggles.Length)
            slotIndex = _focused >= 0 ? _focused : 0;

        _born = true;
        Open(slotIndex);
    }

    /// <summary>名前待ちの妖精がいる鉢(0-2)。いなければ -1。</summary>
    private int PendingBornSlot() {
        var pending = FairySaveBridge.FindUnnamed();
        if (pending == null) return -1;

        int slot = pending.bornSlotIndex;
        if (potToggles == null || slot < 0 || slot >= potToggles.Length) slot = 0;   // 古いセーブ用の保険
        return slot;
    }

    /// <summary>全景の鉢まわり(芽と「生まれた！」)を今の状態に合わせる</summary>
    private void RefreshPots() {
        if (potToggles == null) return;

        int bornSlot = PendingBornSlot();

        for (int i = 0; i < potToggles.Length; i++) {
            bool born = (i == bornSlot);

            // 芽は育成中の鉢と、生まれたけどまだ名前が無い鉢に出す
            if (potSprouts != null && i < potSprouts.Length && potSprouts[i] != null)
                potSprouts[i].SetActive(IsPlanted(i) || born);

            // 「生まれた！」は全景の時だけ(アップでは大きい方を出す)
            if (bornLabels != null && i < bornLabels.Length && bornLabels[i] != null)
                bornLabels[i].SetActive(born && _focused < 0);
        }
    }

    //--------------------------------------------------------------------------
    // 中身
    //--------------------------------------------------------------------------

    private bool IsPlanted(int index) {
        return seedTimes != null && index < seedTimes.Length
               && seedTimes[index] != null && seedTimes[index].IsPlanted;
    }

    private RectTransform TimerOf(int index) {
        if (seedTimes == null || index >= seedTimes.Length || seedTimes[index] == null) return null;
        return seedTimes[index].transform as RectTransform;
    }

    private RectTransform ReduceOf(int index) {
        if (reduceButtons == null || index >= reduceButtons.Length || reduceButtons[index] == null) return null;
        return reduceButtons[index].transform as RectTransform;
    }

    /// <summary>アップ画面へ持っていったものを全景の位置へ返す</summary>
    private void RestoreAll() {
        for (int i = 0; i < _timerLayouts.Length; i++) {
            if (_timerLayouts[i] != null) _timerLayouts[i].Restore();
            if (_reduceLayouts[i] != null) _reduceLayouts[i].Restore();

            // 短縮ボタンはアップ画面だけのもの(全景では鉢の下に時間だけ)
            if (reduceButtons != null && i < reduceButtons.Length && reduceButtons[i] != null)
                reduceButtons[i].SetActive(false);
        }
    }

    /// <summary>
    /// 鉢のトグルを本来の状態へ揃える。
    /// 見ている鉢は ON、それ以外は「植わっているか」に合わせる
    /// (植わっている鉢が ON = 全景でも鉢の下に残り時間が出る)。
    /// </summary>
    private void SyncToggles(int focusedIndex) {
        if (potToggles == null) return;

        _suppress = true;
        for (int i = 0; i < potToggles.Length; i++) {
            if (potToggles[i] == null) continue;

            bool want = (i == focusedIndex) || IsPlanted(i);
            if (potToggles[i].isOn != want) potToggles[i].isOn = want;
        }
        _suppress = false;
    }

    //--------------------------------------------------------------------------

    /// <summary>全景での置き場所を控えておくための入れ物</summary>
    private class Layout {
        private readonly RectTransform _rt;
        private readonly Transform _parent;
        private readonly int _sibling;
        private readonly Vector2 _anchorMin, _anchorMax, _pivot, _anchoredPos, _size;
        private readonly Vector3 _scale;

        public Layout(RectTransform rt) {
            _rt = rt;
            _parent = rt.parent;
            _sibling = rt.GetSiblingIndex();
            _anchorMin = rt.anchorMin;
            _anchorMax = rt.anchorMax;
            _pivot = rt.pivot;
            _anchoredPos = rt.anchoredPosition;
            _size = rt.sizeDelta;
            _scale = rt.localScale;
        }

        /// <summary>置き場所の中央へ移す</summary>
        public void MoveTo(RectTransform slot, float scaleMultiplier) {
            if (_rt == null || slot == null) return;
            if (_rt.parent == slot) return;

            _rt.SetParent(slot, false);
            _rt.anchorMin = new Vector2(0.5f, 0.5f);
            _rt.anchorMax = new Vector2(0.5f, 0.5f);
            _rt.pivot = new Vector2(0.5f, 0.5f);
            _rt.anchoredPosition = Vector2.zero;
            _rt.sizeDelta = _size;
            _rt.localScale = _scale * scaleMultiplier;
        }

        /// <summary>全景の位置へ返す</summary>
        public void Restore() {
            if (_rt == null || _parent == null) return;
            if (_rt.parent == _parent) return;

            _rt.SetParent(_parent, false);
            _rt.SetSiblingIndex(_sibling);
            _rt.anchorMin = _anchorMin;
            _rt.anchorMax = _anchorMax;
            _rt.pivot = _pivot;
            _rt.anchoredPosition = _anchoredPos;
            _rt.sizeDelta = _size;
            _rt.localScale = _scale;
        }
    }
}
