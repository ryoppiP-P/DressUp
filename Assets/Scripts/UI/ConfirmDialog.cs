//==============================================================================
//  File   : ConfirmDialog.cs
//  Brief  : 汎用の確認ダイアログ(メッセージ + はい/いいえ)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/7
//------------------------------------------------------------------------------
//  DeleteConfirmDialog(セーブ削除専用)と同じ作りの汎用版。
//  「コーデを適用していません」等、削除以外の確認にはこちらを使う。
//==============================================================================
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmDialog : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("メッセージ")]
    [SerializeField] private TMP_Text messageText;

    [Header("ボタン")]
    [SerializeField] private Button yesButton;   // 実行する
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
    /// <param name="message">表示するメッセージ(省略時はInspectorで設定済みの文言のまま)</param>
    public void Open(Action onConfirmed, string message = null) {
        _onConfirmed = onConfirmed;
        if (!string.IsNullOrEmpty(message) && messageText != null) messageText.text = message;
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
