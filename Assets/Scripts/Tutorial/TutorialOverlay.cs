//==============================================================================
//  File   : TutorialOverlay.cs
//  Brief  : チュートリアル演出の見た目(暗転+穴あき ハイライト、神様の台詞ボックス)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  上下左右4枚の暗幕(Button付き)で画面を覆い、対象を指定すると
//  その部分だけ穴を開けて実物のボタンにタップを通す(ハイライト)。
//  対象を指定しなければ4枚で画面全体を覆う(=全暗転。台詞のタップ待ちに使う)。
//
//  TutorialManager から呼ばれる想定で、シーンをまたいで常駐する
//  (TutorialManagerと同じ DontDestroyOnLoad オブジェクトの子)。
//==============================================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialOverlay : MonoBehaviour {
    [Header("自分自身(暗幕たちの親)のRectTransform")]
    [SerializeField] private RectTransform canvasRect;

    [Header("暗幕(上下左右)。対象の周りだけ穴を開けて残りを覆う")]
    [SerializeField] private Button maskTop;
    [SerializeField] private Button maskBottom;
    [SerializeField] private Button maskLeft;
    [SerializeField] private Button maskRight;

    [Header("神様の台詞ボックス")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text messageText;

    [Header("ハイライトの余白(px)")]
    [SerializeField] private float highlightPadding = 16f;

    private bool _advanceTapped;

    void Awake() {
        gameObject.SetActive(false);
        if (maskTop) maskTop.onClick.AddListener(OnMaskTapped);
        if (maskBottom) maskBottom.onClick.AddListener(OnMaskTapped);
        if (maskLeft) maskLeft.onClick.AddListener(OnMaskTapped);
        if (maskRight) maskRight.onClick.AddListener(OnMaskTapped);
    }

    private void OnMaskTapped() {
        _advanceTapped = true;
    }

    /// <summary>神様の台詞を1行表示し、画面タップで進むまで待つ(全暗転・ハイライト無し)</summary>
    public IEnumerator PlayMessage(string speaker, string message) {
        gameObject.SetActive(true);
        ApplyMask(null);
        if (dialogueBox) dialogueBox.SetActive(true);
        if (speakerText) speakerText.text = speaker;
        if (messageText) messageText.text = message;

        _advanceTapped = false;
        yield return null; // 直前のタップを拾わないよう1フレーム待つ
        yield return new WaitUntil(() => _advanceTapped);
    }

    /// <summary>指定した実物のUIだけ穴を開けて見せる(タップは実物へ通す。台詞は任意)</summary>
    public void ShowHighlight(RectTransform target, string speaker = null, string message = null) {
        gameObject.SetActive(true);
        bool showText = !string.IsNullOrEmpty(message);
        if (dialogueBox) dialogueBox.SetActive(showText);
        if (showText) {
            if (speakerText) speakerText.text = speaker;
            if (messageText) messageText.text = message;
        }
        ApplyMask(target);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    //--------------------------------------------------------------------------

    private void ApplyMask(RectTransform target) {
        Rect hole = target != null ? GetPaddedLocalRect(target) : default;
        Rect full = canvasRect.rect;

        SetBar(maskTop, full.xMin, full.xMax, hole.yMax, full.yMax);
        SetBar(maskBottom, full.xMin, full.xMax, full.yMin, hole.yMin);
        SetBar(maskLeft, full.xMin, hole.xMin, hole.yMin, hole.yMax);
        SetBar(maskRight, hole.xMax, full.xMax, hole.yMin, hole.yMax);
    }

    private void SetBar(Button bar, float xMin, float xMax, float yMin, float yMax) {
        if (bar == null) return;
        var rt = (RectTransform)bar.transform;
        float w = Mathf.Max(0f, xMax - xMin);
        float h = Mathf.Max(0f, yMax - yMin);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);
        bar.gameObject.SetActive(w > 0.5f && h > 0.5f);
    }

    // 対象のRectTransformを、このオーバーレイのローカル座標(余白付き)に変換する
    private Rect GetPaddedLocalRect(RectTransform target) {
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        var cam = Camera.main;

        Vector2 min = WorldToLocal(corners[0], cam);
        Vector2 max = WorldToLocal(corners[2], cam);

        return Rect.MinMaxRect(
            Mathf.Min(min.x, max.x) - highlightPadding,
            Mathf.Min(min.y, max.y) - highlightPadding,
            Mathf.Max(min.x, max.x) + highlightPadding,
            Mathf.Max(min.y, max.y) + highlightPadding);
    }

    private Vector2 WorldToLocal(Vector3 world, Camera cam) {
        Vector2 screen = cam != null ? RectTransformUtility.WorldToScreenPoint(cam, world) : (Vector2)world;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out var local);
        return local;
    }
}
