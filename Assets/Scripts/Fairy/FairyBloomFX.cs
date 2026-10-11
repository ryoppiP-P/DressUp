//==============================================================================
//  File   : FairyBloomFX.cs
//  Brief  : 花が咲いた時の演出(後ろの光/リング/花びらとキラキラの噴き出し)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/26
//------------------------------------------------------------------------------
//  画像素材は使わず、丸/リング/星の絵をコードで作ってUI Imageで出す。
//  花本体の拡大アニメは FairyPotFocus 側が行い、ここは周りの飾りだけを担当する。
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FairyBloomFX : MonoBehaviour {
    private static Sprite _soft, _ring, _star;

    private RectTransform _glowLayer, _fxLayer;
    private Image _glow;
    private Coroutine _particleRoutine;
    private readonly List<Particle> _particles = new List<Particle>();

    private class Particle {
        public RectTransform rt;
        public Image img;
        public Vector2 pos, vel;
        public float age, life, spin, baseAlpha, startScale, endScale, gravity, drag;
        public bool isRing;
    }

    /// <summary>花の後ろに光の層、前に飾りの層を用意する</summary>
    public void Begin(RectTransform flower) {
        Clear();
        var parent = flower.parent as RectTransform;
        if (parent == null) return;

        _glowLayer = MakeLayer(parent, "BloomGlow", flower.GetSiblingIndex());
        _fxLayer = MakeLayer(parent, "BloomFX", flower.GetSiblingIndex() + 1);

        _glow = MakeImage(_glowLayer, "Glow", Soft(), new Color(1f, 0.93f, 0.6f, 0f), 1100f);
        Follow(flower);
    }

    /// <summary>花の中心に光と飾りの層を合わせる(毎フレーム呼ぶ)</summary>
    public void Follow(RectTransform flower) {
        if (flower == null) return;
        Vector3 center = flower.TransformPoint(flower.rect.center);
        if (_glowLayer != null) _glowLayer.position = center;
        if (_fxLayer != null) _fxLayer.position = center;
    }

    /// <summary>後ろの光の強さ(0-1)と大きさの倍率</summary>
    public void SetGlow(float alpha, float scale) {
        if (_glow == null) return;
        var c = _glow.color; c.a = Mathf.Clamp01(alpha); _glow.color = c;
        _glow.rectTransform.localScale = Vector3.one * scale;
    }

    /// <summary>咲いた瞬間の噴き出し(リング2つ + 花びら/キラキラ)</summary>
    public void Burst(RectTransform flower) {
        if (_fxLayer == null) return;
        Follow(flower);

        for (int i = 0; i < 2; i++) {
            var ring = NewParticle(Ring(), new Color(1f, 0.95f, 0.75f, 1f), 400f);
            ring.isRing = true;
            ring.life = 0.85f;
            ring.age = -0.12f * i;       // 2つ目は少し遅れて出す
            ring.baseAlpha = 0.85f;
            ring.startScale = 0.3f; ring.endScale = 3.2f;
        }

        Color[] palette = {
            new Color(1f, 0.72f, 0.82f), new Color(1f, 0.88f, 0.4f),
            new Color(1f, 1f, 1f),       new Color(0.75f, 0.95f, 0.7f),
        };

        for (int i = 0; i < 34; i++) {
            bool star = i % 3 == 0;
            var p = NewParticle(star ? Star() : Soft(), palette[Random.Range(0, palette.Length)],
                                star ? Random.Range(40f, 70f) : Random.Range(22f, 44f));
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(350f, 850f);
            p.vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed + Vector2.up * 250f;
            p.life = Random.Range(1.0f, 1.7f);
            p.spin = Random.Range(-240f, 240f);
            p.baseAlpha = 1f;
            p.startScale = 1f; p.endScale = 0.3f;
            p.gravity = star ? 250f : 900f;
            p.drag = 1.6f;
            if (!star) p.rt.sizeDelta = new Vector2(p.rt.sizeDelta.x * 0.55f, p.rt.sizeDelta.y * 1.1f);   // 花びらは縦長
        }

        if (_particleRoutine == null) _particleRoutine = StartCoroutine(RunParticles());
    }

    /// <summary>飾りを全部消す</summary>
    public void Clear() {
        if (_particleRoutine != null) { StopCoroutine(_particleRoutine); _particleRoutine = null; }
        _particles.Clear();
        if (_glowLayer != null) Destroy(_glowLayer.gameObject);
        if (_fxLayer != null) Destroy(_fxLayer.gameObject);
        _glowLayer = null; _fxLayer = null; _glow = null;
    }

    void OnDisable() { Clear(); }

    //--------------------------------------------------------------------------

    private IEnumerator RunParticles() {
        while (_particles.Count > 0) {
            float dt = Time.deltaTime;

            for (int i = _particles.Count - 1; i >= 0; i--) {
                var p = _particles[i];
                p.age += dt;
                if (p.age < 0f) continue;

                float k = p.age / p.life;
                if (k >= 1f) {
                    if (p.rt != null) Destroy(p.rt.gameObject);
                    _particles.RemoveAt(i);
                    continue;
                }

                float fade = k < 0.15f ? k / 0.15f : 1f - Mathf.Clamp01((k - 0.6f) / 0.4f);
                if (p.isRing) fade = 1f - k;
                var c = p.img.color; c.a = p.baseAlpha * fade; p.img.color = c;
                p.rt.localScale = Vector3.one * Mathf.Lerp(p.startScale, p.endScale, k);

                if (!p.isRing) {
                    p.vel.y -= p.gravity * dt;
                    p.vel *= Mathf.Max(0f, 1f - p.drag * dt);
                    p.pos += p.vel * dt;
                    p.rt.anchoredPosition = p.pos;
                    p.rt.Rotate(0f, 0f, p.spin * dt);
                }
            }
            yield return null;
        }
        _particleRoutine = null;
    }

    private Particle NewParticle(Sprite sprite, Color color, float size) {
        color.a = 0f;
        var img = MakeImage(_fxLayer, "P", sprite, color, size);
        var p = new Particle { rt = img.rectTransform, img = img, baseAlpha = 1f, startScale = 1f, endScale = 1f, life = 1f };
        p.rt.anchoredPosition = Vector2.zero;
        _particles.Add(p);
        return p;
    }

    private static RectTransform MakeLayer(RectTransform parent, string name, int siblingIndex) {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.zero;
        rt.SetSiblingIndex(siblingIndex);
        return rt;
    }

    private static Image MakeImage(RectTransform parent, string name, Sprite sprite, Color color, float size) {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(size, size);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    //--------------------------------------------------------------------------
    // 絵をコードで作る(64x64。中心からの距離/形でアルファだけ決める)
    //--------------------------------------------------------------------------

    private static Sprite Soft() { if (_soft == null) _soft = Build(a => Mathf.Pow(Mathf.Clamp01(1f - a.magnitude), 1.6f)); return _soft; }

    private static Sprite Ring() {
        if (_ring == null) _ring = Build(a => {
            float d = Mathf.Abs(a.magnitude - 0.88f);
            return Mathf.Clamp01(1f - d / 0.09f);
        });
        return _ring;
    }

    private static Sprite Star() {
        if (_star == null) _star = Build(a => Mathf.Clamp01(1f - (Mathf.Sqrt(Mathf.Abs(a.x)) + Mathf.Sqrt(Mathf.Abs(a.y))) * 0.95f) * 1.4f);
        return _star;
    }

    private static Sprite Build(System.Func<Vector2, float> alphaAt) {
        const int N = 64;
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[N * N];
        for (int y = 0; y < N; y++) {
            for (int x = 0; x < N; x++) {
                var p = new Vector2((x + 0.5f) / N * 2f - 1f, (y + 0.5f) / N * 2f - 1f);
                pixels[y * N + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alphaAt(p)));
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
    }
}
