//==============================================================================
//  File   : MenuPanel.cs
//  Brief  : 設定/メニュー画面のルート制御(サブ画面の切り替え・開閉)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/2
//------------------------------------------------------------------------------
//  Mission 側の MissionPanel と同じ構成方針で作成。
//  トップ(環境設定/アイテム一覧/ヘルプの入口)と各サブ画面はすべて
//  この MenuPanel の子として配置し、SetActive の切り替えのみで遷移する。
//==============================================================================
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour {
    [Header("パネル本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("メニュートップ")]
    [SerializeField] private GameObject mainMenuRoot;      // トップ本体(戻ってきた時に表示)
    [SerializeField] private Button environmentButton;     // 環境設定へ
    [SerializeField] private Button itemListButton;        // アイテム一覧へ
    [SerializeField] private Button helpButton;             // ヘルプへ
    [SerializeField] private Button backButton;             // ゲームに戻る

    [Header("サブ画面")]
    [SerializeField] private EnvironmentSettingsPanel environmentPanel;
    [SerializeField] private ItemListPanel itemListPanel;
    [SerializeField] private HelpPanel helpPanel;

    void Start() {
        if (environmentButton) environmentButton.onClick.AddListener(OpenEnvironment);
        if (itemListButton) itemListButton.onClick.AddListener(OpenItemList);
        if (helpButton) helpButton.onClick.AddListener(OpenHelp);
        if (backButton) backButton.onClick.AddListener(Close);
    }

    /// <summary>メニュー画面を開く(外部の設定ボタンから呼ぶ)</summary>
    public void Open() {
        if (panelRoot) panelRoot.SetActive(true);
        ShowMain();
    }

    /// <summary>メニュー画面を閉じてゲームに戻る</summary>
    public void Close() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    /// <summary>メニュートップへ戻る(各サブ画面の「戻る」ボタンから呼ばれる)</summary>
    public void ShowMain() {
        SetSubScreen(MenuScreen.Main);
    }

    private void OpenEnvironment() => SetSubScreen(MenuScreen.Environment);
    private void OpenItemList() => SetSubScreen(MenuScreen.ItemList);
    private void OpenHelp() => SetSubScreen(MenuScreen.Help);

    //--------------------------------------------------------------
    // サブ画面切り替え(1つだけ開いて他は閉じる)
    //--------------------------------------------------------------
    private void SetSubScreen(MenuScreen screen) {
        if (mainMenuRoot) mainMenuRoot.SetActive(screen == MenuScreen.Main);

        SetPanelOpen(environmentPanel, screen == MenuScreen.Environment);
        SetPanelOpen(itemListPanel, screen == MenuScreen.ItemList);
        SetPanelOpen(helpPanel, screen == MenuScreen.Help);
    }

    private void SetPanelOpen(EnvironmentSettingsPanel panel, bool open) {
        if (panel == null) return;
        if (open) panel.Open(); else panel.Close();
    }

    private void SetPanelOpen(ItemListPanel panel, bool open) {
        if (panel == null) return;
        if (open) panel.Open(); else panel.Close();
    }

    private void SetPanelOpen(HelpPanel panel, bool open) {
        if (panel == null) return;
        if (open) panel.Open(); else panel.Close();
    }
}
