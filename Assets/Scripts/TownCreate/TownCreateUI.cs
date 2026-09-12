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
    }

    private void RefreshLabel() {
        if (modeLabel == null) return;
        string text = "モード:\nなし";
        if (controller.Mode == TownEditMode.Place) text = "モード:\n配置";
        else if (controller.Mode == TownEditMode.Delete) text = "モード:\n削除";
        modeLabel.text = text;
    }
}
