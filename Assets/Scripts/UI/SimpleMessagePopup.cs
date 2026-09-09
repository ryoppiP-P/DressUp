//==============================================================================
//  File   : SimpleMessagePopup.cs
//  Brief  : 一言メッセージを出して自動で消える汎用トースト(FairyMessagePopupの汎用版)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/7
//------------------------------------------------------------------------------
//  「適用OK！」「お金が足りないよ！」など、閉じるボタン不要な一言案内に使う。
//  シーンごとにパネルを1つ置いて、必要な場所からShow(message)を呼ぶだけでよい。
//==============================================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleMessagePopup : MonoBehaviour {
    [Header("開閉するパネル(このスクリプトとは別のオブジェクトにすること)")]
    [SerializeField] private GameObject panelRoot;

    [Header("表示")]
    [SerializeField] private TMP_Text messageText;

    [Header("閉じるボタン(任意)")]
    [SerializeField] private Button closeButton;

    [Header("自動で閉じるまでの秒数(0なら閉じない)")]
    [SerializeField] private float autoCloseSeconds = 1.5f;

    private Coroutine _closing;

    void Awake() {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    void OnDestroy() {
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
    }

    /// <summary>メッセージを出す</summary>
    public void Show(string message) {
        if (string.IsNullOrEmpty(message)) return;

        if (messageText != null) messageText.text = message;
        if (panelRoot != null) panelRoot.SetActive(true);

        if (_closing != null) StopCoroutine(_closing);
        if (autoCloseSeconds > 0f && isActiveAndEnabled) _closing = StartCoroutine(CloseLater());
    }

    public void Close() {
        if (_closing != null) { StopCoroutine(_closing); _closing = null; }
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private IEnumerator CloseLater() {
        yield return new WaitForSeconds(autoCloseSeconds);
        Close();
    }
}
