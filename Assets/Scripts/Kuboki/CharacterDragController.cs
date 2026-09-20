//==============================================================================
//  File   : CharacterDragController.cs
//  Brief  : 街のキャラをドラッグして好きな場所に置く。近くに置いたら会話を持ちかける
//
//  Author : Ryoto Kikuchi
//------------------------------------------------------------------------------
//  ・キャラに触れたらすぐつかむ(長押し不要)。つかんでいる間はカメラのパン/ズームを止める
//    (CameraController が IsDragging を見ている)
//  ・離した場所がそのキャラの新しい居場所になる(TownPositionSaver が定期的にセーブ)。
//    数秒その場で待ってから、今いる場所から一番近い WayPoint へ戻って通常の巡回に復帰する
//  ・別のキャラの近くで離すと、すれ違いと同じ「！」ポップアップが出る(タップで会話)
//
//  ルートのコライダーは0.2x0.2しか無く触りにくいので、つかむ判定は
//  キャラの見た目の範囲(pickHalfSize)を自前で判定している。
//  頭上の報酬ポップアップに触れた時は、つかまずにポップアップのタップを優先する。
//==============================================================================
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)] // CameraController より先に IsDragging を確定させる
public class CharacterDragController : MonoBehaviour {
    /// <summary>今キャラをドラッグ中か(カメラ操作を止めるために CameraController が参照)</summary>
    public static bool IsDragging { get; private set; }

    [Header("つかむ判定(キャラの原点から見た、見た目の範囲の半分の大きさ)")]
    [SerializeField] private Vector2 pickHalfSize = new Vector2(1.3f, 1.9f);    // 実測: 見た目は原点から x[-1.4,1.35] / y[-1.8,2.0]
    [SerializeField] private Vector2 pickCenterOffset = new Vector2(0f, 0.1f);

    [Header("離したあと")]
    [Tooltip("この距離以内に別のキャラが居たら会話を持ちかける")]
    [SerializeField] private float talkRange = 2.5f;
    [Tooltip("会話にならなかった時、その場で待ってから歩き出すまでの秒数")]
    [SerializeField] private float waitAfterDropSeconds = 4f;

    private Camera _cam;
    private CharacterManager _held;
    private Vector3 _grabOffset;   // つかんだ点とキャラの原点のずれ(瞬間移動して見えないように)

    void Awake() {
        _cam = Camera.main;
    }

    void OnDisable() {
        // シーンを離れる/無効化された時に、つかんだままにならないようにする
        if (_held != null) Release(_held.transform.position, false);
        IsDragging = false;
    }

    void Update() {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        if (!ReadPointer(out Vector2 screenPos, out bool pressedNow, out bool isDown, out bool releasedNow, out int touchId)) {
            if (_held != null) Release(_held.transform.position, false); // 入力が途切れたら離した扱い
            return;
        }

        if (_held == null) {
            if (pressedNow) TryPick(screenPos, touchId);
            return;
        }

        Vector3 world = ScreenToWorld(screenPos);
        Vector3 p = world + _grabOffset;
        p.z = _held.transform.position.z;
        _held.transform.position = p;

        if (releasedNow || !isDown) Release(p, true);
    }

    //--------------------------------------------------------------------------
    // つかむ
    //--------------------------------------------------------------------------

    private void TryPick(Vector2 screenPos, int touchId) {
        if (TownCreateController.IsEditScreenOpen) return;
        if (TalkManager.Instance != null && TalkManager.Instance.IsBusy) return; // 誘い/会話中はつかませない
        if (IsPointerOverUI(touchId)) return;
        if (IsMultiTouch()) return; // 2本指はズーム操作

        Vector3 world = ScreenToWorld(screenPos);

        // 頭上の報酬ポップアップに触れているなら、つかまずにそちらのタップを優先する
        foreach (var col in Physics2D.OverlapPointAll(world))
            if (col.GetComponent<RewardPopupTap>() != null) return;

        CharacterManager best = null;
        foreach (var cm in FindObjectsByType<CharacterManager>(FindObjectsSortMode.None)) {
            if (!IsPickable(cm)) continue;

            Vector2 center = (Vector2)cm.transform.position + pickCenterOffset;
            if (Mathf.Abs(world.x - center.x) > pickHalfSize.x) continue;
            if (Mathf.Abs(world.y - center.y) > pickHalfSize.y) continue;

            // 重なっていたら手前(Yが低い方)のキャラを優先(描画順と同じ)
            if (best == null || cm.transform.position.y < best.transform.position.y) best = cm;
        }
        if (best == null) return;

        _held = best;
        _grabOffset = best.transform.position - world;
        _grabOffset.z = 0f;
        IsDragging = true;

        best.SetHeld(true);
        var wander = best.GetComponent<TownWander>();
        if (wander != null) wander.HoldFor(waitAfterDropSeconds); // 立ち姿(Idle)にする
    }

    private static bool IsPickable(CharacterManager cm) {
        if (cm == null || !cm.gameObject.activeInHierarchy) return false;
        if (cm.IsPaused) return false; // 会話中・誘い中・すれ違い停止中は触れない
        var view = cm.GetComponent<Character>();
        if (view == null || TownPositionSaver.IsIgnored(view)) return false; // UI表示用のキャラは対象外
        return true;
    }

    //--------------------------------------------------------------------------
    // 離す
    //--------------------------------------------------------------------------

    private void Release(Vector3 dropPos, bool allowTalk) {
        var cm = _held;
        _held = null;
        IsDragging = false;
        if (cm == null) return;

        cm.SetHeld(false);

        var wander = cm.GetComponent<TownWander>();
        if (wander != null) wander.HoldFor(waitAfterDropSeconds);

        if (!allowTalk) return;

        var target = FindTalkTarget(cm);
        if (target != null) cm.TryStartActiveConversation(target);
    }

    // 離した場所の近くに居る、話しかけられる相手のうち一番近いキャラ
    private CharacterManager FindTalkTarget(CharacterManager dropped) {
        CharacterManager best = null;
        float bestDist = talkRange;

        foreach (var other in FindObjectsByType<CharacterManager>(FindObjectsSortMode.None)) {
            if (other == dropped || !IsPickable(other)) continue;

            float d = Vector2.Distance(dropped.transform.position, other.transform.position);
            if (d > bestDist) continue;

            bestDist = d;
            best = other;
        }
        return best;
    }

    //--------------------------------------------------------------------------
    // 入力
    //--------------------------------------------------------------------------

    private Vector3 ScreenToWorld(Vector2 screenPos) {
        Vector3 w = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(_cam.transform.position.z)));
        w.z = 0f;
        return w;
    }

    // 画面に触れている指(なければマウス)の状態を返す
    private static bool ReadPointer(out Vector2 pos, out bool pressedNow, out bool isDown, out bool releasedNow, out int touchId) {
        pos = default; pressedNow = false; isDown = false; releasedNow = false; touchId = 0;

        var ts = Touchscreen.current;
        if (ts != null) {
            var t = ts.primaryTouch;
            bool active = t.press.isPressed || t.press.wasPressedThisFrame || t.press.wasReleasedThisFrame;
            if (active) {
                pos = t.position.ReadValue();
                pressedNow = t.press.wasPressedThisFrame;
                isDown = t.press.isPressed;
                releasedNow = t.press.wasReleasedThisFrame;
                touchId = t.touchId.ReadValue();
                return true;
            }
        }

        var ms = Mouse.current;
        if (ms != null) {
            pos = ms.position.ReadValue();
            pressedNow = ms.leftButton.wasPressedThisFrame;
            isDown = ms.leftButton.isPressed;
            releasedNow = ms.leftButton.wasReleasedThisFrame;
            return isDown || releasedNow || pressedNow;
        }

        return false;
    }

    private static bool IsMultiTouch() {
        var ts = Touchscreen.current;
        if (ts == null) return false;

        int pressed = 0;
        foreach (var t in ts.touches) if (t.press.isPressed) pressed++;
        return pressed >= 2;
    }

    private static bool IsPointerOverUI(int touchId) {
        if (EventSystem.current == null) return false;
        return touchId != 0
            ? EventSystem.current.IsPointerOverGameObject(touchId)
            : EventSystem.current.IsPointerOverGameObject();
    }
}
