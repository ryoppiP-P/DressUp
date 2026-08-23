//==============================================================================
//  File   : FairyBirthWatcher.cs
//  Brief  : 妖精の畑で「育ちきった種」を見張り、誕生の演出を出す
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;

public class FairyBirthWatcher : MonoBehaviour {
    [Header("誕生演出のポップアップ")]
    [SerializeField] private BirthPopup popup;

    [Header("育ちきったかを見に行く間隔(秒)")]
    [SerializeField] private float checkInterval = 1f;

    [Header("生まれた時に自動で鉢のアップを開く")]
    [SerializeField] private bool showAutomatically = true;

    private float _timer;
    private string _shownFor = "";   // もう自動で見せた妖精(戻るの後に出しっぱなしにしないため)

    void Start() {
        Check();
    }

    void Update() {
        if (popup != null && popup.IsShowing) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        _timer = checkInterval;
        Check();
    }

    /// <summary>育ちきった種を妖精にして、まだ名前が無い子がいれば誕生演出を出す</summary>
    public void Check() {
        if (popup == null || popup.IsShowing) return;
        if (SaveManager.Instance == null) return;

        // 名前を付ける前に中断していた妖精がいれば、そちらを優先して再開する
        var pending = FairySaveBridge.FindUnnamed();

        if (pending == null) pending = HatchReadySlot();
        if (pending == null) return;

        if (!showAutomatically) return;
        if (_shownFor == pending.characterId) return;   // 一度見せた子は鉢から開き直してもらう

        _shownFor = pending.characterId;
        popup.Show(pending.characterId, pending.bornSlotIndex);
    }

    /// <summary>育ちきったスロットがあれば妖精にして返す。無ければ null。</summary>
    private FairyRosterEntry HatchReadySlot() {
        for (int i = 0; i < FairySaveBridge.SlotCount; i++) {
            if (!FairySaveBridge.IsReadyToHatch(i)) continue;

            var entry = FairySaveBridge.HatchSlot(i);
            if (entry == null) continue;

            Debug.Log($"[FairyBirthWatcher] slot{i} から {entry.characterId} が誕生");
            return entry;
        }
        return null;
    }
}
