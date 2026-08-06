//==============================================================================
//  File   : DeleteConfirmDialog.cs
//  Brief  : セーブデータ削除の確認ダイアログ(はい/いいえ)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//  誤タップでのデータ消失を防ぐため、削除の実行は必ずこのダイアログを経由する。
//==============================================================================
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeleteConfirmDialog : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("メッセージ")]
    [SerializeField] private TMP_Text messageText;

    [Header("ボタン")]
    [SerializeField] private Button yesButton;   // 削除する
    [SerializeField] private Button noButton;    // やめる

    // 「はい」が押された時に実行するコールバック
    private Action _onConfirmed;

    void Awake() {
        if (yesButton) yesButton.onClick.AddListener(OnClickYes);
        if (noButton) noButton.onClick.AddListener(OnClickNo);
    }

    /// <summary>
    /// 確認ダイアログを開く。
    /// </summary>
    /// <param name="onConfirmed">「はい」が押された時に実行する処理</param>
    public void Open(Action onConfirmed) {
        _onConfirmed = onConfirmed;
        if (panelRoot) panelRoot.SetActive(true);
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    private void OnClickYes() {
        Close();
        var callback = _onConfirmed;
        _onConfirmed = null;
        callback?.Invoke(); // Close の後に実行(シーン遷移などを安全に行うため)
    }

    private void OnClickNo() {
        Close();
        _onConfirmed = null;
    }
}
