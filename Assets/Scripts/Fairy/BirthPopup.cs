//==============================================================================
//  File   : BirthPopup.cs
//  Brief  : 「誕生！」の演出。確認すると着せ替え画面(キャラクリ)へ進む
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//==============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BirthPopup : MonoBehaviour {
    [Header("ポップアップ本体(開閉対象)")]
    [SerializeField] private GameObject panelRoot;

    [Header("表示")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private string message = "誕生！";

    [Header("次へ進むボタン")]
    [SerializeField] private Button confirmButton;

    [Header("進む先(キャラクリ = 着せ替え画面)")]
    [SerializeField] private string dressUpSceneName = "KisekaeScene";

    private string _characterId;

    public bool IsShowing { get; private set; }

    void Awake() {
        if (panelRoot) panelRoot.SetActive(false);
    }

    void Start() {
        if (confirmButton) confirmButton.onClick.AddListener(GoToDressUp);
    }

    /// <summary>誕生した妖精を指定して演出を出す</summary>
    public void Show(string characterId) {
        _characterId = characterId;
        IsShowing = true;

        if (panelRoot) panelRoot.SetActive(true);
        if (messageText) messageText.text = message;
    }

    public void GoToDressUp() {
        if (string.IsNullOrEmpty(_characterId)) return;

        // 着せ替え画面がこのIDのキャラを読み込むようにしてから遷移する
        FairyBirthFlow.Begin(_characterId);
        CharacterSelection.SelectedId = _characterId;

        SceneManager.LoadScene(dressUpSceneName);
    }
}
