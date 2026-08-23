//==============================================================================
//  File   : FairyCloseUpBack.cs
//  Brief  : 植木鉢のアップ画面の「戻る」
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/23
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;
using UnityEngine.UI;

public class FairyCloseUpBack : MonoBehaviour {
    [Header("戻るボタン")]
    [SerializeField] private Button backButton;

    [Header("鉢のトグル(3つ)")]
    [SerializeField] private Toggle[] potToggles;

    void Awake() {
        if (backButton != null) backButton.onClick.AddListener(Back);
    }

    void OnDestroy() {
        if (backButton != null) backButton.onClick.RemoveListener(Back);
    }

    /// <summary>開いている鉢のトグルを戻す(= seedManager がアップ画面を閉じる)</summary>
    public void Back() {
        if (potToggles == null) return;

        foreach (var toggle in potToggles) {
            if (toggle != null && toggle.isOn) toggle.isOn = false;
        }
    }
}
