//==============================================================================
//  File   : HelpEntrySlot.cs
//  Brief  : ヘルプ画面の1行分(アコーディオン: タイトルタップで詳細を開閉)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpEntrySlot : MonoBehaviour {
    [Header("タイトル部")]
    [SerializeField] private Button titleButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private RectTransform arrowIcon; // 開閉に合わせて回転させる矢印(任意・無くても可)

    [Header("詳細部")]
    [SerializeField] private GameObject detailRoot;
    [SerializeField] private TMP_Text detailText;

    private bool _isOpen;

    void Awake() {
        if (titleButton) titleButton.onClick.AddListener(Toggle);
    }

    /// <summary>タイトル/詳細文言をセットし、閉じた状態から表示を開始する</summary>
    public void Setup(string title, string detail) {
        if (titleText) titleText.text = title;
        if (detailText) detailText.text = detail;
        SetOpen(false);
    }

    private void Toggle() => SetOpen(!_isOpen);

    private void SetOpen(bool open) {
        _isOpen = open;
        if (detailRoot) detailRoot.SetActive(open);
        if (arrowIcon) arrowIcon.localEulerAngles = new Vector3(0f, 0f, open ? 180f : 0f);
    }
}
