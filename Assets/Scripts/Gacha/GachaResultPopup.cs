//==============================================================================
//  File   : GachaResultPopup.cs
//  Brief  : ガチャ結果ポップアップ(1回分/10回分の抽選結果をまとめて表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3 (2026/9/9 演出追加: 1件ずつ順番に明かす演出)
//------------------------------------------------------------------------------
//  抽選結果は保存しない(見せるだけ)。閉じたら消える。
//  結果は全部いきなり出さず、Cover で隠した状態で並べてから1件ずつ
//  ずらしたタイミングで明かしていく(GachaResultSlot.PlayReveal())。
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultPopup : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("結果一覧")]
    [SerializeField] private GachaResultSlot slotPrefab;
    [SerializeField] private Transform contentParent; // ScrollView の Content

    [Header("閉じるボタン")]
    [SerializeField] private Button closeButton;

    [Header("演出(1件ずつ明かす間隔)")]
    [SerializeField] private float revealInterval = 0.15f;

    private readonly List<GachaResultSlot> _spawned = new();
    private Coroutine _revealCoroutine;

    void Awake() {
        if (closeButton) closeButton.onClick.AddListener(Close);
    }

    /// <summary>抽選結果を表示する(1件ずつ順番に明かしていく)</summary>
    public void Show(List<GachaEntry> results) {
        foreach (var slot in _spawned) Destroy(slot.gameObject);
        _spawned.Clear();

        if (_revealCoroutine != null) { StopCoroutine(_revealCoroutine); _revealCoroutine = null; }

        if (results != null) {
            foreach (var entry in results) {
                var slot = Instantiate(slotPrefab, contentParent);
                slot.Setup(entry); // 見た目は Cover の下に隠れた状態で並ぶ
                _spawned.Add(slot);
            }
        }

        if (panelRoot) panelRoot.SetActive(true);

        if (isActiveAndEnabled) _revealCoroutine = StartCoroutine(RevealSequence());
    }

    private IEnumerator RevealSequence() {
        foreach (var slot in _spawned) {
            if (slot != null) StartCoroutine(slot.PlayReveal());
            yield return new WaitForSeconds(revealInterval);
        }
        _revealCoroutine = null;
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }
}
