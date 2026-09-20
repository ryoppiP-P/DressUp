//==============================================================================
//  File   : MissionCharacterView.cs
//  Brief  : ミッション画面の上の空きに、持っているコをランダムで1体出す
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/21
//------------------------------------------------------------------------------
//  出すのは FairySaveBridge.GetNamedFairies()(畑で生まれて名前を付け終わったコ)。
//  画面を開くたびに選び直す(このスクリプトを画面本体に付けるので OnEnable が走る)。
//
//  静止画を1枚焼くのではなく、専用カメラを RenderTexture に描きっぱなしにして
//  RawImage で映している。こうするとキャラがそのまま Idle で動く。
//  (毎フレーム焼き直す方式だと RenderTexture と Texture2D を作り続けてしまう)
//==============================================================================
using UnityEngine;
using UnityEngine.UI;

public class MissionCharacterView : MonoBehaviour {
    [Header("映す先")]
    [SerializeField] private RawImage display;

    [Header("画面外に置いた撮影用のキャラとカメラ")]
    [SerializeField] private Character previewCharacter;
    [SerializeField] private Camera previewCamera;

    [Header("映像の解像度")]
    [SerializeField] private int textureSize = 512;

    private RenderTexture _rt;

    // 撮影用キャラ/カメラを今使っているビュー。ミッション画面とメニュー画面は同じキャラ/カメラを共有していて、
    // 画面の切り替え時の OnEnable/OnDisable の順番は保証されないので、
    // 「今の持ち主」以外の OnDisable では止めない(止めると、後から開いた側の映像まで消えてしまう)
    private static MissionCharacterView _owner;

    void OnEnable() {
        ShowRandom();
    }

    void OnDisable() {
        if (_owner != this) return;
        _owner = null;

        // 見ていない間は回しっぱなしにしない
        if (previewCamera != null) previewCamera.gameObject.SetActive(false);
        if (previewCharacter != null) previewCharacter.gameObject.SetActive(false);
    }

    void OnDestroy() {
        if (_rt == null) return;

        if (previewCamera != null) previewCamera.targetTexture = null;
        if (display != null) display.texture = null;
        _rt.Release();
        Destroy(_rt);
    }

    /// <summary>持っているコから1体選んで映す</summary>
    public void ShowRandom() {
        if (display == null || previewCharacter == null || previewCamera == null) return;

        var fairies = FairySaveBridge.GetNamedFairies();
        if (fairies.Count == 0) {
            display.enabled = false;   // まだ誰もいない時は何も出さない
            previewCamera.gameObject.SetActive(false);
            previewCharacter.gameObject.SetActive(false);
            return;
        }

        _owner = this;
        EnsureTexture();

        previewCharacter.gameObject.SetActive(true);
        previewCamera.gameObject.SetActive(true);

        var entry = fairies[Random.Range(0, fairies.Count)];
        previewCharacter.SetCharacterId(entry.characterId);
        previewCharacter.ReloadForId();
        previewCharacter.SetAnimate(true);              // Idle で動かす
        previewCharacter.SetState(CharaState.Idle);

        display.enabled = true;
    }

    private void EnsureTexture() {
        if (_rt == null) {
            _rt = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
            _rt.Create();
        }

        // カメラを共有する別のビューが先に開いていても、自分の映像を映せるよう毎回つなぎ直す
        previewCamera.targetTexture = _rt;
        display.texture = _rt;
    }
}
