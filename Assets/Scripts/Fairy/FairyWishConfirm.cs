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
using System.Collections;
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

    [Header("植えるのに使う種(ショップで買うもの)")]
    [SerializeField] private OtherItem seedItem;

    [Header("植えられない時に出すメッセージ(任意)")]
    [SerializeField] private FairyMessagePopup message;

    [Header("落ちてくる種(SeedAnimation が動かすもの)。落ちきってから育成中の見た目にする")]
    [SerializeField] private GameObject fallingSeed;

    [Header("種が落ちてこなかった時に待つのをやめる秒数")]
    [SerializeField] private float maxWaitSeconds = 5f;

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

        // 種を1つ使う。持っていない時とキャラが上限に達している時はここで止まる。
        string reason;
        if (!FairyFarmRules.TryUseSeed(seedItem, out reason)) {
            Debug.Log("[FairyWishConfirm] 植えられない: " + reason);
            if (message != null) message.Show(reason);
            _pendingKeywords.Clear();
            return;
        }

        var personality = FairyKeywordTable.Build(_pendingKeywords);
        float growSeconds = ResolveGrowSeconds();

        FairySaveBridge.PlantSeed(slotIndex, growSeconds, _pendingKeywords, personality);

        Debug.Log($"[FairyWishConfirm] slot{slotIndex} に種を植えた " +
                  $"({string.Join("/", _pendingKeywords)} / {growSeconds}秒)");

        _pendingKeywords.Clear();

        // 種が落ちきってから、開いている鉢のアップを「育成中」の見た目に切り替える
        // (キーワードを引っ込めて、鉢の下に大きく残り時間と短縮ボタンを出す)
        StartCoroutine(ShowGrowingAfterSeedLands());
    }

    /// <summary>
    /// 久保木さんの SeedAnimation が種を落とし終える(落ちた種を非表示に戻す)のを待ってから、
    /// 鉢のアップを育成中の見た目にする。設計書でも「種が落ちる → タイマーが出る」の順。
    /// </summary>
    private IEnumerator ShowGrowingAfterSeedLands() {
        if (fallingSeed != null) {
            yield return null;   // 同じ「はい」で種が表示されるので、1フレーム待ってから見る

            float limit = Time.time + maxWaitSeconds;
            while (fallingSeed.activeInHierarchy && Time.time < limit) yield return null;
        }

        var focus = FairyPotFocus.Current;
        if (focus != null) focus.Refresh();
    }

    private int ResolveSlotIndex() {
        if (targetSeed != null && !targetSeed.IsPlanted) return targetSeed.SlotIndex;

        // 今プレイヤーが開いている鉢に植える。
        // これが無いと常に一番若い空きスロットへ植わってしまい、
        // 鉢2や鉢3をタップしても中身が鉢1に入って「植わっていない鉢」が残る。
        var focus = FairyPotFocus.Current;
        if (focus != null) {
            int focused = focus.FocusedSlot;
            if (focused >= 0 && !FairySaveBridge.IsPlanted(focused)) return focused;
        }

        return FairySaveBridge.FindEmptySlot();
    }

    private float ResolveGrowSeconds() {
        if (overrideGrowSeconds > 0f) return overrideGrowSeconds;
        if (targetSeed != null && targetSeed.GrowSeconds > 0f) return targetSeed.GrowSeconds;

        // targetSeed を指定していない時(空きスロットに自動で植える運用)は、
        // シーンにある SeedTime から育成時間を取る。
        // これを見ないと下の保険値が使われてしまい、seedTimeSet の設定が効かなかった。
        foreach (var seedTime in FindObjectsByType<SeedTime>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (seedTime != null && seedTime.GrowSeconds > 0f) return seedTime.GrowSeconds;
        }

        return 3600f; // 保険(1時間)
    }
}
