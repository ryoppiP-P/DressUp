//==============================================================================
//  File   : MissionPanel.cs
//  Brief  : ミッションUIの全体表示（タブ切り替え・スクロールビュー）
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/6/18
//------------------------------------------------------------------------------
//==============================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionPanel : MonoBehaviour {
    [Header("行の生成")]
    [SerializeField] private MissionSlot slotPrefab;
    [SerializeField] private Transform contentParent; // ScrollView の Content

    [Header("タブ")]
    [SerializeField] private Button dailyTab;
    [SerializeField] private Button weeklyTab;
    [SerializeField] private Button challengeTab;

    [Header("下部ボタン")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button claimAllButton;

    [Header("この画面自体（戻るで閉じる対象）")]
    [SerializeField] private GameObject panelRoot;

    // 「挑戦する」を押した時にどこへ連れて行くか。
    // パネルを指定すればこのシーン内でそれを開き、シーン名を指定すればそちらへ移動する。
    // どちらも空なら、ミッション画面を閉じるだけ(街に戻る)。
    [System.Serializable]
    public class ChallengeDestination {
        public MissionType type;
        [Tooltip("同じシーン内で開くパネル")] public GameObject targetPanel;
        [Tooltip("シーンごと移動する場合の行き先")] [BuildScene] public string targetSceneName;
    }

    [Header("「挑戦する」の行き先")]
    [SerializeField] private List<ChallengeDestination> challengeDestinations = new();

    private readonly List<MissionSlot> _spawned = new();
    private MissionCategory _current = MissionCategory.Daily;

    void OnEnable() {
        if (MissionManager.Instance == null) return;

        MissionManager.Instance.OnMissionChanged += RefreshAll;

        // 妖精の誕生・服の入手・プレイ時間は別のシーンで増えるので、
        // 開いた時にセーブから数え直して表示を合わせる
        MissionManager.Instance.SyncDerivedProgress();

        // 閉じている間に進んだぶん(報酬回収・ガチャ等)を表示に反映する。
        // SyncDerivedProgress は「変化があった時だけ」通知を飛ばすので、
        // それ任せにすると閉じる前の数字のまま出てしまう。
        RefreshAll();
    }

    void OnDisable() {
        if (MissionManager.Instance != null)
            MissionManager.Instance.OnMissionChanged -= RefreshAll;
    }

    void Start() {
        // タブ
        if (dailyTab) dailyTab.onClick.AddListener(() => ShowCategory(MissionCategory.Daily));
        if (weeklyTab) weeklyTab.onClick.AddListener(() => ShowCategory(MissionCategory.Weekly));
        if (challengeTab) challengeTab.onClick.AddListener(() => ShowCategory(MissionCategory.Challenge));

        // 下部ボタン
        if (backButton) backButton.onClick.AddListener(Close);
        if (claimAllButton) claimAllButton.onClick.AddListener(OnClickClaimAll);

        // 初期表示はデイリー
        ShowCategory(MissionCategory.Daily);
    }

    // カテゴリを表示（タブ切り替え）
    public void ShowCategory(MissionCategory category) {
        _current = category;
        Rebuild();
    }

    // 行を作り直す
    private void Rebuild() {
        // 既存を消す
        foreach (var s in _spawned) Destroy(s.gameObject);
        _spawned.Clear();

        if (MissionManager.Instance == null) return;

        var missions = MissionManager.Instance.GetMissions(_current);
        foreach (var m in missions) {
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(m, this);
            _spawned.Add(slot);
        }
    }

    // 進捗が変わったとき：作り直さず各行を更新（軽い）
    private void RefreshAll() {
        foreach (var s in _spawned) s.Refresh();
    }

    private void OnClickClaimAll() {
        if (MissionManager.Instance == null) return;
        MissionManager.Instance.ClaimAll(_current);
        // ClaimAll 内で OnMissionChanged が飛ぶので表示は自動更新される
    }

    /// <summary>「挑戦する」を押された。そのミッションの場所へ連れて行く。</summary>
    public void GoToChallenge(MissionData mission) {
        if (mission == null) return;

        var dest = challengeDestinations.Find(d => d != null && d.type == mission.type);

        // 行き先が決まっていないものは、閉じて街に戻るだけ
        if (dest == null) { Close(); return; }

        if (!string.IsNullOrEmpty(dest.targetSceneName)) {
            UnityEngine.SceneManagement.SceneManager.LoadScene(dest.targetSceneName);
            return;
        }

        Close();
        if (dest.targetPanel != null) dest.targetPanel.SetActive(true);
    }

    // 画面を開く（外部から呼ぶ用）
    public void Open() {
        if (panelRoot) panelRoot.SetActive(true);
        ShowCategory(MissionCategory.Daily);
    }

    // 画面を閉じる（戻る）
    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }
}
