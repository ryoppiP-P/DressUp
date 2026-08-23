//==============================================================================
//  File   : FairyMessagePopup.cs
//  Brief  : 妖精の畑で「植えられない理由」を出す小さなお知らせ
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/23
//------------------------------------------------------------------------------
//==============================================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FairyMessagePopup : MonoBehaviour {
    [Header("開閉するパネル(このスクリプトとは別のオブジェクトにすること)")]
    [SerializeField] private GameObject panelRoot;

    [Header("表示")]
    [SerializeField] private TMP_Text messageText;

    [Header("閉じるボタン(任意)")]
    [SerializeField] private Button closeButton;

    [Header("自動で閉じるまでの秒数(0なら閉じない)")]
    [SerializeField] private float autoCloseSeconds = 3f;

    private Coroutine _closing;

    void Awake() {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    void OnDestroy() {
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
    }

    /// <summary>お知らせを出す</summary>
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
