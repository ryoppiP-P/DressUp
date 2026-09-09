//==============================================================================
//  File   : GachaResultSlot.cs
//  Brief  : ガチャ結果ポップアップの1枠分の表示(アイコン・名前・レアリティ)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3 (2026/9/9 演出追加: カバー→ポップイン+レアリティ別フラッシュ)
//------------------------------------------------------------------------------
//  名前は showItemName で出し分ける。今は OFF にして、そのぶんアイテムの絵を
//  枠いっぱいに大きく見せている。名前を戻したい時は Inspector で ON にすれば、
//  下に名前の場所を空けたレイアウトへ自動で戻る(NameText は消していない)。
//
//  演出: Setup() の時点では絵を Cover で隠しておき、PlayReveal() が呼ばれた
//  タイミングでカバーが弾け→アイコンがバウンドしながらポップイン→
//  レアリティに応じた色のフラッシュが弾ける、という流れにしている。
//  SR ほど演出を大きく/長くして「おっ！」感を出す(バカゲー方針: [[nadonezu-bakage-tone]])。
//==============================================================================
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaResultSlot : MonoBehaviour {
    [Header("表示")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image rarityBadge;
    [SerializeField] private RarityIconTable rarityTable; // DressUp側と共通のレアリティアイコン表

    [Header("名前の表示")]
    [Tooltip("OFFにすると名前を隠し、そのぶんアイテムの絵を枠いっぱいに出す")]
    [SerializeField] private bool showItemName = false;

    [Header("アイコンの配置")]
    [Tooltip("枠の内側に取る余白")]
    [SerializeField] private float iconMargin = 12f;
    [Tooltip("名前を出す時に下へ空ける高さ")]
    [SerializeField] private float nameAreaHeight = 60f;

    [Header("演出(結果が明かされるまで隠すカバー)")]
    [SerializeField] private GameObject coverRoot;      // 「？」などを出した、中身を隠す板
    [SerializeField] private Image flashImage;           // 明かした瞬間に弾けるフラッシュ(初期alpha0)

    [Header("演出のレアリティ別カラー")]
    [SerializeField] private Color normalFlashColor = Color.white;
    [SerializeField] private Color rareFlashColor = new Color(0.55f, 0.8f, 1f, 1f);
    [SerializeField] private Color superRareFlashColor = new Color(1f, 0.85f, 0.3f, 1f);

    private GachaEntry _entry;

    /// <summary>抽選結果の1件分をセットする(見た目は Cover の下に隠しておく)</summary>
    public void Setup(GachaEntry entry) {
        if (entry == null || entry.item == null) return;
        _entry = entry;
        var item = entry.item;

        if (iconImage) { iconImage.sprite = item.icon; iconImage.enabled = item.icon != null; }

        // 名前は隠していても中身は入れておく(表示を戻した時にそのまま出るように)
        if (nameText) {
            nameText.text = item.itemName;
            nameText.gameObject.SetActive(showItemName);
        }
        ApplyIconLayout();

        if (rarityBadge) {
            var sprite = rarityTable != null ? rarityTable.GetIcon(item.rarity) : null;
            rarityBadge.sprite = sprite;
            rarityBadge.enabled = sprite != null;
        }

        // 明かされるまでは絵を隠しておく(PlayReveal() が呼ばれるまでこの状態のまま)
        if (iconImage) iconImage.transform.localScale = Vector3.zero;
        if (rarityBadge) rarityBadge.gameObject.SetActive(false);
        if (coverRoot) coverRoot.SetActive(true);
        if (flashImage) {
            var c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
            flashImage.gameObject.SetActive(true);
        }
    }

    /// <summary>カバーを弾いて中身を明かす演出。レアリティが高いほど大きく/長く見せる。</summary>
    public IEnumerator PlayReveal() {
        Rarity rarity = _entry != null && _entry.item != null ? _entry.item.rarity : Rarity.Normal;
        bool big = rarity == Rarity.SuperRare;
        bool mid = rarity == Rarity.Rare;

        // カバーが弾ける(縮んで消える)
        if (coverRoot != null) {
            var coverRt = coverRoot.transform;
            float coverT = 0f;
            const float coverDuration = 0.12f;
            while (coverT < coverDuration) {
                coverT += Time.deltaTime;
                float p = Mathf.Clamp01(coverT / coverDuration);
                coverRt.localScale = Vector3.one * (1f - p);
                yield return null;
            }
            coverRoot.SetActive(false);
        }

        if (rarityBadge) rarityBadge.gameObject.SetActive(true);

        // フラッシュ(レアリティで色/大きさ/長さを変える。SRは特に派手に)
        if (flashImage) {
            flashImage.color = big ? superRareFlashColor : mid ? rareFlashColor : normalFlashColor;
            float flashDuration = big ? 0.5f : mid ? 0.4f : 0.3f;
            float maxScale = big ? 2.6f : mid ? 2.0f : 1.5f;
            StartCoroutine(FlashRoutine(flashImage, flashDuration, maxScale));
        }

        // アイコンがバウンドしながらポップイン(back-ease-out。SRほど大げさにオーバーシュートさせる)
        if (iconImage != null) {
            float overshoot = big ? 2.4f : mid ? 1.9f : 1.5f;
            float popDuration = big ? 0.45f : mid ? 0.35f : 0.25f;
            var iconRt = iconImage.transform;
            float t = 0f;
            while (t < popDuration) {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / popDuration);
                float scale = BackEaseOut(p, overshoot);
                iconRt.localScale = Vector3.one * scale;
                yield return null;
            }
            iconRt.localScale = Vector3.one;
        }
    }

    private static IEnumerator FlashRoutine(Image image, float duration, float maxScale) {
        var rt = image.transform;
        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            // 前半で一気に広がり、そのまま透明になっていく(打ち上げ花火っぽい弾け方)
            float scale = Mathf.Lerp(0.2f, maxScale, Mathf.Sqrt(p));
            rt.localScale = Vector3.one * scale;

            var c = image.color;
            c.a = 1f - p;
            image.color = c;

            yield return null;
        }

        var endColor = image.color;
        endColor.a = 0f;
        image.color = endColor;
        image.gameObject.SetActive(false);
    }

    // back-easing-out: 1を少し超えてから収まる、弾むようなイージング
    private static float BackEaseOut(float t, float overshoot) {
        t -= 1f;
        return t * t * ((overshoot + 1f) * t + overshoot) + 1f;
    }

    // 名前を出すかどうかでアイコンの大きさを変える
    private void ApplyIconLayout() {
        if (iconImage == null) return;

        var rect = iconImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(iconMargin, iconMargin + (showItemName ? nameAreaHeight : 0f));
        rect.offsetMax = new Vector2(-iconMargin, -iconMargin);
    }

#if UNITY_EDITOR
    // Inspector で切り替えた時にシーン上でもすぐ反映されるように。
    // OnValidate の中で RectTransform を書き換えると Unity に怒られる
    // (SendMessage cannot be called during OnValidate)ので、1フレーム遅らせる。
    private void OnValidate() {
        UnityEditor.EditorApplication.delayCall += () => {
            if (this == null) return;

            if (nameText) nameText.gameObject.SetActive(showItemName);
            ApplyIconLayout();
        };
    }
#endif
}
