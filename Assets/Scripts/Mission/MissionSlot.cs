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

    public void Setup(MissionData data) {
        _data = data;
        description.text = data.description;

        // どんぐり報酬（0なら非表示）
        if (nutRewardObj != null) {
            bool hasNut = data.rewardNut > 0;
            nutRewardObj.SetActive(hasNut);
            if (hasNut && nutRewardText != null)
                nutRewardText.text = $"×{data.rewardNut}";
        }

        // はちみつ報酬（0なら非表示）
        if (honeyRewardObj != null) {
            bool hasHoney = data.rewardHoney > 0;
            honeyRewardObj.SetActive(hasHoney);
            if (hasHoney && honeyRewardText != null)
                honeyRewardText.text = $"×{data.rewardHoney}";
        }

        Refresh();

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnClickAction);
    }

    public void Refresh() {
        if (MissionManager.Instance == null || _data == null) return;

        int progress = MissionManager.Instance.GetProgress(_data);
        int target = _data.targetCount;
        var state = MissionManager.Instance.GetState(_data);

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
            // 「挑戦する」→ 該当ページへ移動、など（仕様の“挑戦する”）
            // ここは遷移先が決まってから実装。今はログだけ。
            Debug.Log($"[Mission] 挑戦する: {_data.description}");
        }
        // Claimed は interactable=false なので押せない
    }
}
