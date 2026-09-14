//==============================================================================
//  File   : TownCreateUI.cs
//  Brief  : 街クリエイトの配置モード/削除モード切り替えUIの配線
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TownCreateUI : MonoBehaviour {
    [SerializeField] private TownCreateController controller;
    [SerializeField] private Button placeButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text modeLabel;
    [SerializeField] private GameObject decorationRow;
    [SerializeField] private Button[] decorationButtons;

    [Header("街クリボタンからの入退場")]
    [SerializeField] private GameObject editModeRoot;   // このUI一式(見せる/隠す対象)
    [SerializeField] private GameObject normalBottomBar; // 通常時のTownScene側BottomBar
    [SerializeField] private GameObject blockedLayerRoot; // 配置不可の赤ハッチング(編集中だけ見せる)

    void Start() {
        if (placeButton) placeButton.onClick.AddListener(OnClickPlace);
        if (deleteButton) deleteButton.onClick.AddListener(OnClickDelete);
        if (exitButton) exitButton.onClick.AddListener(OnClickExit);

        if (decorationButtons != null) {
            for (int i = 0; i < decorationButtons.Length; i++) {
                int index = i;
                if (decorationButtons[i] != null)
                    decorationButtons[i].onClick.AddListener(() => controller.SelectDecoration(index));
            }
        }

        RefreshLabel();
    }

    /// <summary>街クリボタンから呼ぶ。編集画面を出し、通常のボトムバーを隠す。</summary>
    public void EnterEditMode() {
        if (editModeRoot) editModeRoot.SetActive(true);
        if (normalBottomBar) normalBottomBar.SetActive(false);
        if (blockedLayerRoot) blockedLayerRoot.SetActive(true);
        controller.SetMode(TownEditMode.None);
        TownCreateController.IsEditScreenOpen = true;
        if (decorationRow) decorationRow.SetActive(false);
        RefreshLabel();
    }

    private void OnClickPlace() {
        controller.SetMode(TownEditMode.Place);
        if (decorationRow) decorationRow.SetActive(true);
        RefreshLabel();
    }

    private void OnClickDelete() {
        controller.SetMode(TownEditMode.Delete);
        if (decorationRow) decorationRow.SetActive(false);
        RefreshLabel();
    }

    private void OnClickExit() {
        controller.SetMode(TownEditMode.None);
        if (decorationRow) decorationRow.SetActive(false);
        RefreshLabel();

        // 街クリ画面自体を閉じて、通常のボトムバーへ戻る
        if (editModeRoot) editModeRoot.SetActive(false);
        if (normalBottomBar) normalBottomBar.SetActive(true);
        if (blockedLayerRoot) blockedLayerRoot.SetActive(false);
        TownCreateController.IsEditScreenOpen = false;
    }

    private void RefreshLabel() {
        if (modeLabel == null) return;
        string text = "モード:\nなし";
        if (controller.Mode == TownEditMode.Place) text = "モード:\n配置";
        else if (controller.Mode == TownEditMode.Delete) text = "モード:\n削除";
        modeLabel.text = text;
    }
}
