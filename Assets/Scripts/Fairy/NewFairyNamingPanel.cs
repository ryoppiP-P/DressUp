//==============================================================================
//  File   : NewFairyNamingPanel.cs
//  Brief  : 着せ替え画面で、生まれたての妖精に名前を付けて街へ送り出す
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  誕生フローの時だけ動く。普段の着せ替えでは何もしない。
//  流れは「着せ替えて満足したら戻るボタン → 名前入力が出る → 決定で街へ」。
//  決定を押すと「今のコーデを確定 → 名前を保存 → 名簿に名付け済みの印 → 街へ」。
//  名付けが終わるまで街には出ないので、途中でアプリを落としても
//  次に畑へ行けば続きから再開できる。
//==============================================================================
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewFairyNamingPanel : MonoBehaviour {
    [Header("パネル本体(誕生フローの時だけ表示)")]
    [SerializeField] private GameObject panelRoot;

    [Header("入力")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button confirmButton;

    [Header("メッセージ")]
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private string emptyNameMessage = "なまえを いれてね";

    [Header("送り先(街)")]
    [SerializeField] private string townSceneName = "TownMoveTest";

    [Header("戻るボタン(誕生フロー中はここから名前入力を出す)")]
    [SerializeField] private Button backButton;

    private Character Target {
        get { return DressUpTarget.Instance != null ? DressUpTarget.Instance.Current : null; }
    }

    // 誕生フロー中に「戻る」で街へ帰らせない仕組みは、ボタン側の DressUpHomeGuard が
    // FairyBirthFlow.IsNamingFlow を見て自分から何もしないようにする形で持っている
    // (このスクリプトはその間だけ backButton に名前入力を開く処理を足す側)。

    void Start() {
        // 名前入力は最初は出さない。着せ替えが終わって戻るボタンを押した時に出す。
        if (panelRoot) panelRoot.SetActive(false);
        if (errorText) errorText.text = "";

        if (!FairyBirthFlow.IsNamingFlow) return;

        if (backButton) backButton.onClick.AddListener(OpenPanel);
        if (confirmButton) confirmButton.onClick.AddListener(Confirm);
    }

    /// <summary>戻るボタンから呼ばれる。名前入力を表示する。</summary>
    public void OpenPanel() {
        if (!FairyBirthFlow.IsNamingFlow) return;

        if (panelRoot) panelRoot.SetActive(true);
        if (errorText) errorText.text = "";
    }

    public void Confirm() {
        string characterId = FairyBirthFlow.PendingNamingId;
        if (string.IsNullOrEmpty(characterId)) return;

        string fairyName = nameInput != null ? nameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(fairyName)) {
            if (errorText) errorText.text = emptyNameMessage;
            return;
        }

        var character = Target;
        if (character != null) {
            character.ApplyOutfit();               // 着せ替え中の見た目を確定して保存
            character.SetDisplayName(fairyName);   // 名前はここでセーブされる
        } else if (SaveManager.Instance != null) {
            SaveManager.Instance.SetCharacterName(characterId, fairyName);
        }

        FairySaveBridge.MarkNamingDone(characterId); // これで街に出るようになる
        FairyBirthFlow.Finish();

        SceneManager.LoadScene(townSceneName);
    }
}
