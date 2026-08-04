//==============================================================================
//  File   : GachaResultPopup.cs
//  Brief  : ガチャ結果ポップアップ(1回分/10回分の抽選結果をまとめて表示)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//  抽選結果は保存しない(見せるだけ)。閉じたら消える。
//==============================================================================
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

    private readonly List<GachaResultSlot> _spawned = new();

    void Awake() {
        if (closeButton) closeButton.onClick.AddListener(Close);
    }

    /// <summary>抽選結果を表示する</summary>
    public void Show(List<GachaEntry> results) {
        foreach (var slot in _spawned) Destroy(slot.gameObject);
        _spawned.Clear();

        if (results != null) {
            foreach (var entry in results) {
                var slot = Instantiate(slotPrefab, contentParent);
                slot.Setup(entry);
                _spawned.Add(slot);
            }
        }

        if (panelRoot) panelRoot.SetActive(true);
    }

    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }
}
