//==============================================================================
//  File   : TownCreateUI.cs
//  Brief  : 街クリエイトの編集画面UI(装飾一覧 / ゴミ箱 / ホームへ戻る)の配線
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  操作:
//   ・装飾一覧はいつも見えている。装飾をタップで選ぶ = 配置モード(マップをタップして置く)
//   ・ゴミ箱を押すと削除モード(マップの装飾をタップして撤去)。
//     もう一度ゴミ箱を押すか、装飾一覧の装飾をタップすると配置モードに戻る
//   ・左下の矢印でホーム画面に戻る
//==============================================================================
using UnityEngine;
using UnityEngine.UI;

public class TownCreateUI : MonoBehaviour {
    [SerializeField] private TownCreateController controller;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject decorationRow;
    [SerializeField] private Button[] decorationButtons;
    [SerializeField] private TMPro.TMP_Text[] decorationCountLabels; // decorationButtonsと同じ並び順

    [Header("選択中の装飾ボタンの大きさ")]
    [SerializeField] private float selectedScale = 1.15f;

    [Header("削除モード中のゴミ箱の大きさ")]
    [SerializeField] private float deleteActiveScale = 1.15f;

    [Header("街クリボタンからの入退場")]
    [SerializeField] private GameObject editModeRoot;   // このUI一式(見せる/隠す対象)
    [SerializeField] private GameObject normalBottomBar; // 通常時のTownScene側BottomBar
    [SerializeField] private GameObject blockedLayerRoot; // 配置不可の赤ハッチング(編集中だけ見せる)

    void Start() {
        if (deleteButton) deleteButton.onClick.AddListener(OnClickDelete);
        if (exitButton) exitButton.onClick.AddListener(OnClickExit);

        if (decorationButtons != null) {
            for (int i = 0; i < decorationButtons.Length; i++) {
                int index = i;
                if (decorationButtons[i] != null) {
                    decorationButtons[i].onClick.AddListener(() => OnClickDecoration(index));
                }
            }
        }

        if (controller != null) controller.OnDecorationCountChanged += RefreshDecorationCounts;
    }

    void OnDestroy() {
        if (controller != null) controller.OnDecorationCountChanged -= RefreshDecorationCounts;
    }

    /// <summary>街クリボタンから呼ぶ。編集画面を出し、通常のボトムバーを隠す。</summary>
    public void EnterEditMode() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySE(SEType.Tap);

        // 開いているMission/Gacha/Shopを閉じ、ボトムバーの矢印も隠す(編集中はしまえない)
        BottomPanelCoordinator.NotifyOpened(CloseEditScreen);

        if (editModeRoot) editModeRoot.SetActive(true);
        if (normalBottomBar) normalBottomBar.SetActive(false);
        if (blockedLayerRoot) blockedLayerRoot.SetActive(true);
        if (decorationRow) decorationRow.SetActive(true);
        TownCreateController.IsEditScreenOpen = true;

        // 入った時は配置モード。持っている最初の装飾を選んでおく
        SelectFirstOwnedDecoration();
        controller.SetMode(TownEditMode.Place);
        RefreshDecorationCounts();
        RefreshHighlights();
    }

    private void SelectFirstOwnedDecoration() {
        var items = controller.DecorationItems;
        if (items == null) return;

        if (controller.SelectedDecoration >= 0 && controller.SelectedDecoration < items.Length
            && items[controller.SelectedDecoration] != null
            && ConsumableBridge.GetCount(items[controller.SelectedDecoration].itemId) > 0) return;

        for (int i = 0; i < items.Length; i++) {
            if (items[i] != null && ConsumableBridge.GetCount(items[i].itemId) > 0) {
                controller.SelectDecoration(i);
                return;
            }
        }
    }

    private void OnClickDecoration(int index) {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySE(SEType.Grab);
        controller.SelectDecoration(index);
        controller.SetMode(TownEditMode.Place);   // 削除モード中ならここで解除される
        RefreshHighlights();
    }

    private void OnClickDelete() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySE(SEType.Tap);
        bool toDelete = controller.Mode != TownEditMode.Delete;
        controller.SetMode(toDelete ? TownEditMode.Delete : TownEditMode.Place);
        RefreshHighlights();
    }

    /// <summary>ホーム画面(通常のボトムバー)へ戻る</summary>
    private void OnClickExit() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySE(SEType.Click);
        CloseEditScreen();
        BottomPanelCoordinator.NotifyClosed(CloseEditScreen);
    }

    // 他のパネルが開いた時にも呼ばれるので、SEやコーディネータへの通知はここでは行わない
    private void CloseEditScreen() {
        controller.SetMode(TownEditMode.None);
        if (editModeRoot) editModeRoot.SetActive(false);
        if (normalBottomBar) normalBottomBar.SetActive(true);
        if (blockedLayerRoot) blockedLayerRoot.SetActive(false);
        TownCreateController.IsEditScreenOpen = false;
        RefreshHighlights();
    }

    /// <summary>
    /// 配置モード中は選んでいる装飾だけ拡大、削除モード中はゴミ箱を拡大する(色は変えない)。
    /// </summary>
    private void RefreshHighlights() {
        bool placing = controller.Mode == TownEditMode.Place;

        if (decorationButtons != null) {
            for (int i = 0; i < decorationButtons.Length; i++) {
                if (decorationButtons[i] == null) continue;
                bool isSelected = placing && i == controller.SelectedDecoration;

                decorationButtons[i].transform.localScale = Vector3.one * (isSelected ? selectedScale : 1f);
            }
        }

        if (deleteButton != null) {
            bool deleting = controller.Mode == TownEditMode.Delete;
            deleteButton.transform.localScale = Vector3.one * (deleting ? deleteActiveScale : 1f);
        }
    }

    /// <summary>装飾ボタンの所持数表示を更新する(0個ならボタンも押せなくする)</summary>
    private void RefreshDecorationCounts() {
        var items = controller.DecorationItems;
        if (items == null) return;

        for (int i = 0; i < items.Length; i++) {
            if (items[i] == null) continue;
            int count = ConsumableBridge.GetCount(items[i].itemId);

            if (decorationCountLabels != null && i < decorationCountLabels.Length && decorationCountLabels[i] != null)
                decorationCountLabels[i].text = count.ToString();

            if (decorationButtons != null && i < decorationButtons.Length && decorationButtons[i] != null)
                decorationButtons[i].interactable = count > 0;
        }
    }
}
