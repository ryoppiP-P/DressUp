//==============================================================================
//  File   : SpeechBubble.cs
//  Brief  : キャラ頭上に出す吹き出しUI(World Space Canvas想定)
//
//  Name   : Ryoto Kikuchi
//
//  ShowLine(text, duration) で指定時間だけ文字を表示し、自動で消える。
//  文字は一文字ずつタイプライター風に出す(表示にかかる合計時間はdurationのまま変わらない)。
//==============================================================================
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class SpeechBubble : MonoBehaviour {
    [SerializeField] private TMP_Text label;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("タイプライター(1文字あたりの間隔・秒)")]
    [SerializeField] private float charInterval = 0.04f;

    private Coroutine _current;

    void Awake() {
        Hide();
    }

    // キャラの左右反転(Character.SetFacingがルートのlocalScale.xを反転させる)の影響を受けないよう、
    // 親のスケール符号を打ち消して見た目が常に正しい向きになるようにする
    void LateUpdate() {
        var parent = transform.parent;
        if (parent == null) return;

        float sign = parent.lossyScale.x < 0f ? -1f : 1f;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * sign;
        transform.localScale = s;
    }

    public void ShowLine(string text, float duration) {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(ShowRoutine(text, duration));
    }

    private IEnumerator ShowRoutine(string text, float duration) {
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // オートサイズは「今表示されている文字数」を見て毎回計算し直すため、
        // タイプライターで1文字ずつ増やしていくと最初の1文字だけ大きく表示され、
        // 文字が増えるにつれ縮んでいくように見えてしまう。
        // 先に全文を仮表示してサイズを確定させ、以降はそのサイズで固定して表示する。
        if (label != null) {
            if (label.enableAutoSizing) {
                label.text = text;
                label.ForceMeshUpdate();
                float settledSize = label.fontSize;
                label.enableAutoSizing = false;
                label.fontSize = settledSize;
            }
            label.text = "";
        }

        // 文字数がどれだけ多くても、表示にかかる合計時間はdurationを超えないようにする
        // (PlayLines側がdurationぶん待ってから次の行へ進むため、ここで超過すると噛み合わなくなる)
        float interval = charInterval;
        if (text.Length > 0) interval = Mathf.Min(interval, duration / text.Length);

        var sb = new StringBuilder();
        for (int i = 0; i < text.Length; i++) {
            sb.Append(text[i]);
            if (label != null) label.text = sb.ToString();

            if (interval > 0f) yield return new WaitForSeconds(interval);
        }

        float remaining = duration - interval * text.Length;
        if (remaining > 0f) yield return new WaitForSeconds(remaining);

        Hide();
    }

    public void Hide() {
        if (_current != null) {
            StopCoroutine(_current);
            _current = null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}
