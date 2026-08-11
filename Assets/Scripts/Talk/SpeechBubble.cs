//==============================================================================
//  File   : SpeechBubble.cs
//  Brief  : キャラ頭上に出す吹き出しUI(World Space Canvas想定)
//
//  Name   : Ryoto Kikuchi
//
//  ShowLine(text, duration) で指定時間だけ文字を表示し、自動で消える。
//==============================================================================
using System.Collections;
using TMPro;
using UnityEngine;

public class SpeechBubble : MonoBehaviour {
    [SerializeField] private TMP_Text label;
    [SerializeField] private CanvasGroup canvasGroup;

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
        if (label != null) label.text = text;
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(duration);

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
