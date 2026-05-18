using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    public float panSpeed = 0.1f;   // 移動速度
    public float zoomSpeed = 0.1f;  // ズーム速度

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // --- 1本指：移動（スワイプ） ---
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            // 2Dの場合（XとYを移動）
            transform.Translate(-touchDeltaPosition.x * panSpeed, -touchDeltaPosition.y * panSpeed, 0);
        }

        // --- 2本指：ズーム（ピンチ） ---
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // 1フレーム前の指の位置を計算
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // 前フレームと今フレームの指同士の距離を計算
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // 距離の差分
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // 直交投影（Orthographic）カメラのサイズを調整
            if (cam.orthographic)
            {
                cam.orthographicSize += deltaMagnitudeDiff * zoomSpeed;
                // サイズが小さくなりすぎないよう制限
                cam.orthographicSize = Mathf.Max(cam.orthographicSize, 0.1f);
            }
        }
    }
}