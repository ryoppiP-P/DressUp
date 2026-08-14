//==============================================================================
//  File   : FairyWishConfirm.cs
//  Brief  : 「願いを込める」(キーワード3つ決定)で性格を作り、種を植える
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  ToggleSlotManager が「3つ選ぶと YES/NO の確認UIを出す」ところまでやっているので、
//  その YES ボタンに相乗りする。
//  YES を押すと ToggleSlotManager 側が選択をリセットしてしまうため、
//  「ちょうど3つ選ばれた時点のキーワード」を毎フレーム控えておき、確定時はそれを使う。
//==============================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FairyWishConfirm : MonoBehaviour {
    [Header("キーワードのトグル(ToggleSelectCheck と同じ8つを割り当てる)")]
    [SerializeField] private Toggle[] keywordToggles;

    [Header("願いを込める(YES)ボタン")]
    [SerializeField] private Button yesButton;

    [Header("植える先。空なら空いているスロットを自動で探す")]
    [SerializeField] private SeedTime targetSeed;

    [Header("育成時間(秒)。未設定なら targetSeed の seedTimeSet を使う")]
    [SerializeField] private float overrideGrowSeconds = 0f;

    // ちょうど3つ選ばれた時点のキーワード(YES を押した時にはもう消えているので控えておく)
    private readonly List<string> _pendingKeywords = new List<string>();

    void Start() {
        if (yesButton != null) yesButton.onClick.AddListener(Confirm);
    }

    void Update() {
        CacheSelectionWhenComplete();
    }

    // ちょうど3つ ON になっている時だけ控えを更新する
    private void CacheSelectionWhenComplete() {
        if (keywordToggles == null) return;

        var on = new List<string>();
        foreach (var toggle in keywordToggles) {
            if (toggle != null && toggle.isOn) on.Add(toggle.gameObject.name);
        }

        if (on.Count != 3) return;

        _pendingKeywords.Clear();
        _pendingKeywords.AddRange(on);
    }

    /// <summary>願いを込める：キーワードから性格を決めて種を植える</summary>
    public void Confirm() {
        if (_pendingKeywords.Count == 0) {
            Debug.LogWarning("[FairyWishConfirm] キーワードが控えられていません(トグルの割り当てを確認)");
            return;
        }

        int slotIndex = ResolveSlotIndex();
        if (slotIndex < 0) {
            Debug.LogWarning("[FairyWishConfirm] 空いている種スロットがありません");
            return;
        }

        var personality = FairyKeywordTable.Build(_pendingKeywords);
        float growSeconds = ResolveGrowSeconds();

        FairySaveBridge.PlantSeed(slotIndex, growSeconds, _pendingKeywords, personality);

        Debug.Log($"[FairyWishConfirm] slot{slotIndex} に種を植えた " +
                  $"({string.Join("/", _pendingKeywords)} / {growSeconds}秒)");

        _pendingKeywords.Clear();
    }

    private int ResolveSlotIndex() {
        if (targetSeed != null && !targetSeed.IsPlanted) return targetSeed.SlotIndex;
        return FairySaveBridge.FindEmptySlot();
    }

    private float ResolveGrowSeconds() {
        if (overrideGrowSeconds > 0f) return overrideGrowSeconds;
        if (targetSeed != null) return targetSeed.GrowSeconds;
        return 300f; // 保険(5分)
    }
}
