using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    [Header("移動")]
    public float panSpeed = 0.01f;

    [Header("ズーム")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    [Header("初期位置")]
    public Vector3 defaultPosition = new Vector3(0, 0, -10);
    public float defaultZoom = 5f;

    // ダブルクリック判定
    private float lastClickTime = 0f;
    private float doubleClickTime = 0.3f;

    private Vector2 lastMousePosition;

    void Start()
    {
        cam = GetComponent<Camera>();

        transform.position = defaultPosition;

        if (cam.orthographic)
        {
            cam.orthographicSize = defaultZoom;
        }
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    // ==================================
    // PC操作
    // ==================================
    void HandleMouse() {
        // UIの上を操作しているならカメラは動かさない
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // --- 押した瞬間：先に初期化する（これが最重要）---
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            lastMousePosition = Mouse.current.position.ReadValue();

            // ダブルクリック判定
            if (Time.time - lastClickTime < doubleClickTime)
                ResetCamera();
            lastClickTime = Time.time;
        }

        // --- 押している間：ドラッグ ---
        if (Mouse.current.leftButton.isPressed) {
            Vector2 current = Mouse.current.position.ReadValue();
            Vector2 delta = current - lastMousePosition;

            transform.Translate(-delta.x * panSpeed, -delta.y * panSpeed, 0);

            lastMousePosition = current; // 毎フレーム更新
        }

        // --- ホイールズーム ---
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0)
            Zoom(-scroll * zoomSpeed * 0.01f);
    }

    // ==================================
    // スマホ操作
    // ==================================
    void HandleTouch()
    {
        if (Touchscreen.current == null) return;

        // 1本目の指がUIの上なら操作しない
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(
                Touchscreen.current.touches[0].touchId.ReadValue()))
            return;

        // タッチ一覧
        var touches = Touchscreen.current.touches;

        // 1本指
        if (touches.Count >= 1 && touches[0].press.isPressed)
        {
            Vector2 delta = touches[0].delta.ReadValue();

            // 横か縦の大きい方だけ動かす
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                transform.Translate(
                    -delta.x * panSpeed,
                    0,
                    0
                );
            }
            else
            {
                transform.Translate(
                    0,
                    -delta.y * panSpeed,
                    0
                );
            }
        }

        // 2本指ズーム
        if (touches.Count >= 2 &&
            touches[0].press.isPressed &&
            touches[1].press.isPressed)
        {
            Vector2 touch0Pos = touches[0].position.ReadValue();
            Vector2 touch1Pos = touches[1].position.ReadValue();

            Vector2 touch0Delta = touches[0].delta.ReadValue();
            Vector2 touch1Delta = touches[1].delta.ReadValue();

            Vector2 prevTouch0Pos = touch0Pos - touch0Delta;
            Vector2 prevTouch1Pos = touch1Pos - touch1Delta;

            float prevMagnitude =
                (prevTouch0Pos - prevTouch1Pos).magnitude;

            float currentMagnitude =
                (touch0Pos - touch1Pos).magnitude;

            float difference = prevMagnitude - currentMagnitude;

            Zoom(difference * zoomSpeed);
        }
    }

    // ==================================
    // ズーム
    // ==================================
    void Zoom(float amount)
    {
        if (cam.orthographic)
        {
            cam.orthographicSize += amount;

            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize,
                minZoom,
                maxZoom
            );
        }
        else
        {
            cam.fieldOfView += amount;

            cam.fieldOfView = Mathf.Clamp(
                cam.fieldOfView,
                minZoom,
                maxZoom
            );
        }
    }

    // ==================================
    // カメラリセット
    // ==================================
    void ResetCamera()
    {
        transform.position = defaultPosition;

        if (cam.orthographic)
        {
            cam.orthographicSize = defaultZoom;
        }
        else
        {
            cam.fieldOfView = defaultZoom;
        }
    }
}