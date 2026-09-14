//==============================================================================
//  File   : TalkPrompt.cs
//  Brief  : すれ違った2人の間に出す「！」ポップアップ(World Space Canvas想定)
//
//  Name   : Ryoto Kikuchi
//
//  TalkManagerがシーンに1つだけ持ち、すれ違いのたびに位置だけ動かして使い回す。
//  タップ検知そのものはInspectorで割り当てたBoxボタン(TapButton)に任せ、
//  ここでは表示/非表示だけを担当する。
//==============================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TalkPrompt : MonoBehaviour {
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button tapButton;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float dotInterval = 0.4f;

    private Coroutine _dotsRoutine;

    /// <summary>タップ検知用のボタン(TalkManagerがonClickを購読する)</summary>
    public Button TapButton => tapButton;

    void Awake() {
        Hide();
    }

    public void Show() {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (_dotsRoutine != null) StopCoroutine(_dotsRoutine);
        _dotsRoutine = StartCoroutine(AnimateDots());
    }

    public void Hide() {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (_dotsRoutine != null) {
            StopCoroutine(_dotsRoutine);
            _dotsRoutine = null;
        }
    }

    // 「・」が0→1→2→3個と増えてから、また0個に戻るのを繰り返す(よくある「考え中」演出)
    private IEnumerator AnimateDots() {
        int count = 0;
        while (true) {
            if (label != null) label.text = new string('・', count);
            yield return new WaitForSeconds(dotInterval);
            count = (count + 1) % 4;
        }
    }
}
