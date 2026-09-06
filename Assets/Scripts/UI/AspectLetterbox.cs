//==============================================================================
//  File   : AspectLetterbox.cs
//  Brief  : 想定アスペクト比(既定9:16)より縦長/横長な端末で、はみ出した分を
//           黒帯で覆うレターボックス
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/8 (2026/9/9 CanvasScaler固定 + カメラviewport絞りを追加)
//------------------------------------------------------------------------------
//  ScreenSpaceOverlay専用のCanvasに、画面四辺ぴったりの黒い帯(上下左右)を敷き、
//  実際にはみ出している辺だけ厚みを持たせる。sortingOrderを最大級にして、
//  そのシーンの他のCanvas(UI/背景)より必ず手前に描画されるようにしている。
//
//  【重要】黒帯を敷くだけでは不十分だった(2026/9/9, Device Simulatorで発覚):
//  1. 既存のCanvasScalerは「Scale With Screen Size / Match: Width」なので、
//     画面がタテに長いほどUIの実効表示領域が1920より下に伸びて増える。
//  2. さらにメインCanvasは ScreenSpaceCamera(カメラ連動)なので、
//     Canvas自体の大きさは「そのカメラのビューポート(画面いっぱい)」のまま。
//  この2つが組み合わさって、BottomBar等の「下端アンカー」なUIが
//  本来の9:16の外＝黒帯の裏側までズレて隠れてしまっていた。
//
//  対策: (a) 該当CanvasScalerを「9:16の箱の幅」だけを基準にした固定scaleFactorへ、
//        (b) ScreenSpaceCameraが参照しているカメラのrect(ビューポート)も
//            同じ9:16の箱にきっちり絞る。
//  (a)(b)を同じ数値(contentWidth/contentHeight)で揃えることで、
//  Canvas自体の実効サイズがぴったり1080x1920になり、UIも背景も黒帯の境界で揃う。
//==============================================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class AspectLetterbox : MonoBehaviour {
    [Header("基準アスペクト比(幅:高さ)。既定は縦長9:16")]
    [SerializeField] private float targetWidth = 9f;
    [SerializeField] private float targetHeight = 16f;

    [Header("黒帯(上下左右)")]
    [SerializeField] private RectTransform barTop;
    [SerializeField] private RectTransform barBottom;
    [SerializeField] private RectTransform barLeft;
    [SerializeField] private RectTransform barRight;

    private int _lastWidth = -1;
    private int _lastHeight = -1;

    // シーン内の「Scale With Screen Size」なCanvasScaler。
    private CanvasScaler[] _scalers;

    // ScreenSpaceCameraなCanvasが参照しているカメラ。ビューポートを絞る対象。
    // (World Space/ScreenSpaceOverlayのCanvasは対象外。SpeechBubble等はWorldSpaceなので触らない)
    private Camera[] _screenSpaceCameras;

    // 絞る前の各カメラの元のrect(このレターボックス自身が無効化された時に戻せるよう保持)
    private readonly Dictionary<Camera, Rect> _originalCameraRects = new Dictionary<Camera, Rect>();

    void Awake() {
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000; // どのシーンのUIよりも必ず手前に出す

        _scalers = FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // ScreenSpaceOverlay(=カメラに紐付いていない)なCanvasは、そのままだと
        // Canvas自体が常に画面いっぱいになり、カメラのrectを絞っても効果が無い。
        // このレターボックス自身のCanvas以外は、メインカメラに紐付くScreenSpaceCameraへ
        // 変換しておく(見た目・クリック判定は同等で、ビューポートだけ絞れるようになる)。
        var mainCam = Camera.main;
        if (mainCam != null) {
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                if (c == canvas) continue;
                if (c.renderMode != RenderMode.ScreenSpaceOverlay) continue;

                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = mainCam;
                c.planeDistance = 100f;
            }
        }

        var cameraSet = new HashSet<Camera>();
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (c.renderMode != RenderMode.ScreenSpaceCamera) continue;
            if (c.worldCamera == null) continue;
            cameraSet.Add(c.worldCamera);
        }
        _screenSpaceCameras = new Camera[cameraSet.Count];
        cameraSet.CopyTo(_screenSpaceCameras);
        foreach (var cam in _screenSpaceCameras) _originalCameraRects[cam] = cam.rect;

        Apply();
    }

    void Update() {
        // 画面回転やウィンドウのリサイズに追従する(値が変わった時だけ計算し直す)
        if (Screen.width == _lastWidth && Screen.height == _lastHeight) return;
        Apply();
    }

    private void Apply() {
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        if (Screen.width <= 0 || Screen.height <= 0) return;

        float targetAspect = targetWidth / targetHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        float barVertical = 0f;   // 上下の帯の高さ(px)
        float barHorizontal = 0f; // 左右の帯の幅(px)
        float contentWidth = Screen.width;
        float contentHeight = Screen.height;

        if (screenAspect < targetAspect) {
            // 画面が基準より縦長(9:16よりさらに細長い) → 幅を基準にして、上下を黒帯にする
            contentHeight = Screen.width / targetAspect;
            barVertical = Mathf.Max(0f, (Screen.height - contentHeight) * 0.5f);
        } else if (screenAspect > targetAspect) {
            // 画面が基準より横長 → 高さを基準にして、左右を黒帯にする
            contentWidth = Screen.height * targetAspect;
            barHorizontal = Mathf.Max(0f, (Screen.width - contentWidth) * 0.5f);
        }

        SetBarHeight(barTop, barVertical);
        SetBarHeight(barBottom, barVertical);
        SetBarWidth(barLeft, barHorizontal);
        SetBarWidth(barRight, barHorizontal);

        LockScalers(contentWidth);
        LockCameraViewports(barHorizontal, barVertical);
    }

    // 「9:16の箱(=黒帯の内側)の幅」だけを基準にscaleFactorを固定する。
    // これで画面がどれだけタテに長くても、UIは1080x1920設計時とまったく同じ
    // 実効サイズで表示される(実効"領域"の方はLockCameraViewportsで絞る)。
    private void LockScalers(float contentWidth) {
        if (_scalers == null) return;

        foreach (var scaler in _scalers) {
            if (scaler == null) continue;
            if (scaler.referenceResolution.x <= 0f) continue;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = contentWidth / scaler.referenceResolution.x;
        }
    }

    // ScreenSpaceCameraなCanvasの実効領域(=そのカメラのビューポート)を、
    // 黒帯の内側にきっちり絞る。これで「下端アンカー」等が黒帯の裏へ伸びなくなる。
    private void LockCameraViewports(float barHorizontal, float barVertical) {
        if (_screenSpaceCameras == null) return;

        float xMin = barHorizontal / Screen.width;
        float xMax = 1f - xMin;
        float yMin = barVertical / Screen.height;
        float yMax = 1f - yMin;

        var rect = new Rect(xMin, yMin, Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));

        foreach (var cam in _screenSpaceCameras) {
            if (cam == null) continue;
            cam.rect = rect;
        }
    }

    private void SetBarHeight(RectTransform bar, float thickness) {
        if (bar == null) return;
        var size = bar.sizeDelta;
        size.y = thickness;
        bar.sizeDelta = size;
        bar.gameObject.SetActive(thickness > 0.5f);
    }

    private void SetBarWidth(RectTransform bar, float thickness) {
        if (bar == null) return;
        var size = bar.sizeDelta;
        size.x = thickness;
        bar.sizeDelta = size;
        bar.gameObject.SetActive(thickness > 0.5f);
    }
}
