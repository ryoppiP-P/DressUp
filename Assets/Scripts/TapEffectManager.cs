using UnityEngine;
using System.Collections.Generic;

public class TapEffectManager : MonoBehaviour {
    public static TapEffectManager Instance { get; private set; }

    [SerializeField] private SpriteRenderer effectPrefab; // 絵を表示するだけのプレハブ
    [SerializeField] private Sprite[] frames;             // スライス済みシート
    [SerializeField] private float frameRate = 24f;

    // 再生中のエフェクトを管理
    private class Playing {
        public SpriteRenderer sr;
        public float timer;
        public int frame;
    }
    private readonly List<Playing> _playing = new();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this); // GameObjectごとではなく、このコンポーネントだけ消す
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update() {
        // 1. タップ監視して生成
        if (Input.GetMouseButtonDown(0)) Spawn(Input.mousePosition);
        for (int i = 0; i < Input.touchCount; i++) {
            var t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began) Spawn(t.position);
        }

        // 2. 再生中エフェクトのコマを進める
        float interval = 1f / Mathf.Max(1f, frameRate);
        for (int i = _playing.Count - 1; i >= 0; i--) {
            var p = _playing[i];
            p.timer += Time.deltaTime;
            if (p.timer < interval) continue;

            p.timer -= interval;
            p.frame++;

            if (p.frame >= frames.Length) {
                Destroy(p.sr.gameObject); // 最後まで再生したら消す
                _playing.RemoveAt(i);
            }
            else {
                p.sr.sprite = frames[p.frame];
            }
        }
    }

    private void Spawn(Vector3 screenPos) {
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPos);
        pos.z = 0f;
        var sr = Instantiate(effectPrefab, pos, Quaternion.identity);
        sr.sprite = frames.Length > 0 ? frames[0] : null;
        _playing.Add(new Playing { sr = sr, timer = 0f, frame = 0 });
    }
}
