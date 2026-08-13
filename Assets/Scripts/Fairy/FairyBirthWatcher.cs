//==============================================================================
//  File   : FairyBirthWatcher.cs
//  Brief  : 妖精の畑で「育ちきった種」を見張り、誕生の演出を出す
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  ・畑を開いた時点で育ちきっていれば、その場で誕生させる
//    (アプリを閉じている間に育っていた場合はここで拾う)
//  ・畑を見ている間に育ちきった場合も、その場で誕生させる
//  ・名前を付ける前にアプリを落とした場合は、名簿に namingDone = false で
//    残っているので、次に畑へ来た時に続きから再開できる
//==============================================================================
using UnityEngine;

public class FairyBirthWatcher : MonoBehaviour {
    [Header("誕生演出のポップアップ")]
    [SerializeField] private BirthPopup popup;

    [Header("育ちきったかを見に行く間隔(秒)")]
    [SerializeField] private float checkInterval = 1f;

    private float _timer;

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

    /// <summary>育ちきった種、または名前待ちの妖精がいれば誕生演出を出す</summary>
    public void Check() {
        if (popup == null || popup.IsShowing) return;
        if (SaveManager.Instance == null) return;

        // 名前を付ける前に中断していた妖精がいれば、そちらを優先して再開する
        var unnamed = FairySaveBridge.FindUnnamed();
        if (unnamed != null) {
            popup.Show(unnamed.characterId);
            return;
        }

        for (int i = 0; i < FairySaveBridge.SlotCount; i++) {
            if (!FairySaveBridge.IsReadyToHatch(i)) continue;

            var entry = FairySaveBridge.HatchSlot(i);
            if (entry == null) continue;

            Debug.Log($"[FairyBirthWatcher] slot{i} から {entry.characterId} が誕生");
            popup.Show(entry.characterId);
            return;
        }
    }
}
