using System.Collections;
using UnityEngine;

public class BottomBarToggle : MonoBehaviour {
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private float hiddenOffsetY = 185f;
    [SerializeField] private float duration = 0.3f;

    private Vector2 _shownPos;
    private bool _hidden;
    private Coroutine _routine;

    void Awake() {
        _shownPos = bottomBar.anchoredPosition;
    }

    public void OnClickArrow() {
        _hidden = !_hidden;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine() {
        Vector2 fromPos = bottomBar.anchoredPosition;
        Vector2 toPos = _hidden ? _shownPos + Vector2.down * hiddenOffsetY : _shownPos;
        float fromRot = arrow.localEulerAngles.z;
        float toRot = _hidden ? 180f : 0f;

        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
            bottomBar.anchoredPosition = Vector2.Lerp(fromPos, toPos, p);
            arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(fromRot, toRot, p));
            yield return null;
        }

        bottomBar.anchoredPosition = toPos;
        arrow.localRotation = Quaternion.Euler(0f, 0f, toRot);
        _routine = null;
    }
}
