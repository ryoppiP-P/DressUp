//==============================================================================
//  File   : MissionSlot.cs
//  Brief  : ミッションUIの1行分の表示
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/6/18
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionSlot : MonoBehaviour {
    [Header("報酬表示")]
    [SerializeField] private GameObject nutRewardObj;    // どんぐりアイコン
    [SerializeField] private TMP_Text nutRewardText;
    [SerializeField] private GameObject honeyRewardObj;  // はちみつアイコン
    [SerializeField] private TMP_Text honeyRewardText;

    [Header("内容")]
    [SerializeField] private TMP_Text description;   // ミッション内容
    [SerializeField] private Slider progressBar;     // 達成率バー
    [SerializeField] private TMP_Text progressText;  // 1/2 など

    [Header("ボタン")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionLabel;   // 挑戦する/受け取る/クリア

    [Header("状態ごとの色")]
    [SerializeField] private Image background;               // 行の背景
    [SerializeField] private Color bgNormal = Color.white;   // 挑戦中・受取可
    [SerializeField] private Color bgClaimed = new Color(0.8f, 0.8f, 0.8f); // クリア(灰)
    [SerializeField] private Color barInProgress = Color.gray;  // 未達成:灰
    [SerializeField] private Color barClaimable = Color.yellow; // 達成:黄
    [SerializeField] private Color barClaimed = Color.gray;     // 受取後:灰

    [Header("報酬アイコン素材")]
    [SerializeField] private Sprite nutSprite;
    [SerializeField] private Sprite honeySprite;

    private MissionData _data;
    private MissionPanel _owner;   // 「挑戦する」でどこへ行くかはこちらが知っている

    public void Setup(MissionData data, MissionPanel owner = null) {
        _data = data;
        _owner = owner;
        description.text = data.description;

        Refresh();   // 報酬の数字は段階で変わるので Refresh 側で出す

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnClickAction);
    }

    public void Refresh() {
        if (MissionManager.Instance == null || _data == null) return;

        int progress = MissionManager.Instance.GetProgress(_data);
        int stage = MissionManager.Instance.GetStage(_data);
        int target = MissionManager.Instance.GetTarget(_data);
        var state = MissionManager.Instance.GetState(_data);

        // 段階制は今の段階の報酬を出す
        int nut = _data.GetRewardNut(stage);
        int honey = _data.GetRewardHoney(stage);
        // 金額のテキストはアイコンの子ではなく兄弟なので、両方まとめて出し入れする
        // (アイコンだけ消すと、報酬0のミッションで古い数字が残ってしまう)
        SetReward(nutRewardObj, nutRewardText, nut);
        SetReward(honeyRewardObj, honeyRewardText, honey);

        // 進捗バー
        progressBar.maxValue = target;
        progressBar.value = progress;
        progressText.text = $"{progress}/{target}";

        // 状態でボタン・色を切り替え
        switch (state) {
            case MissionState.InProgress:
                actionLabel.text = "挑戦する";
                actionButton.interactable = true;
                if (background) background.color = bgNormal;
                SetBarColor(barInProgress);
                break;
            case MissionState.Claimable:
                actionLabel.text = "受け取る";
                actionButton.interactable = true;
                if (background) background.color = bgNormal;
                SetBarColor(barClaimable);
                break;
            case MissionState.Claimed:
                actionLabel.text = "クリア";
                actionButton.interactable = false;
                if (background) background.color = bgClaimed;
                SetBarColor(barClaimed);
                break;
        }
    }

    // 報酬アイコンと金額をまとめて表示/非表示する(0なら丸ごと消す)
    private void SetReward(GameObject icon, TMP_Text amount, int value) {
        bool show = value > 0;

        if (icon != null) icon.SetActive(show);
        if (amount == null) return;

        amount.gameObject.SetActive(show);
        if (show) amount.text = $"×{value}";
    }

    private void SetBarColor(Color c) {
        var fill = progressBar.fillRect ? progressBar.fillRect.GetComponent<Image>() : null;
        if (fill) fill.color = c;
    }

    private void OnClickAction() {
        if (MissionManager.Instance == null || _data == null) return;
        var state = MissionManager.Instance.GetState(_data);

        if (state == MissionState.Claimable) {
            // 受け取る
            MissionManager.Instance.Claim(_data);
        } else if (state == MissionState.InProgress) {
            // 「挑戦する」→ そのミッションの場所へ移動する
            if (_owner != null) _owner.GoToChallenge(_data);
            else Debug.LogWarning("[Mission] 行き先を知っている MissionPanel が渡されていません", this);
        }
        // Claimed は interactable=false なので押せない
    }
}
