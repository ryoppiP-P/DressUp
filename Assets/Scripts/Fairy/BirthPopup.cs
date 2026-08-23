//==============================================================================
//  File   : BirthPopup.cs
//  Brief  : 「生まれた！」の演出。鉢をタッチすると着せ替え画面(キャラクリ)へ進む
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//==============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BirthPopup : MonoBehaviour {
    [Header("演出の本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("表示")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private string message = "生まれた！";

    [Header("着せ替えへ進むボタン(ボタン本体と、鉢のタッチ範囲)")]
    [SerializeField] private Button[] confirmButtons;

    [Header("鉢のアップ画面(生まれた鉢を開いてもらう)")]
    [SerializeField] private FairyPotFocus potFocus;

    [Header("進む先(キャラクリ = 着せ替え画面)")]
    [SerializeField] private string dressUpSceneName = "KisekaeScene";

    private string _characterId;

    public bool IsShowing { get; private set; }

    void Awake() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    void Start() {
        if (confirmButtons == null) return;

        foreach (var button in confirmButtons) {
            if (button != null) button.onClick.AddListener(GoToDressUp);
        }
    }

    /// <summary>誕生した妖精を指定して演出を出す</summary>
    public void Show(string characterId) {
        Show(characterId, -1);
    }

    /// <summary>名前待ちの妖精がいれば、その子で演出を出す(鉢をタップした時)</summary>
    public bool ShowPending() {
        var pending = FairySaveBridge.FindUnnamed();
        if (pending == null) return false;

        Show(pending.characterId, pending.bornSlotIndex);
        return true;
    }

    /// <summary>誕生した妖精と、その子が入っていた鉢(0-2)を指定して演出を出す</summary>
    public void Show(string characterId, int slotIndex) {
        _characterId = characterId;
        IsShowing = true;

        // 鉢のアップにする(生まれた後のスロットは空なのでキーワードは出さない)
        var focus = potFocus != null ? potFocus : FairyPotFocus.Current;
        if (focus != null) focus.OpenForBirth(slotIndex);

        if (panelRoot) panelRoot.SetActive(true);
        if (messageText) messageText.text = message;
    }

    /// <summary>演出を引っ込める(全景へ戻る時。妖精は名前待ちのまま残る)</summary>
    public void Hide() {
        IsShowing = false;
        if (panelRoot) panelRoot.SetActive(false);
    }

    public void GoToDressUp() {
        if (string.IsNullOrEmpty(_characterId)) return;

        // 着せ替え画面がこのIDのキャラを読み込むようにしてから遷移する
        FairyBirthFlow.Begin(_characterId);
        CharacterSelection.SelectedId = _characterId;

        SceneManager.LoadScene(dressUpSceneName);
    }
}
