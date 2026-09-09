//==============================================================================
//  File   : DressUpHomeGuard.cs
//  Brief  : 着せ替え画面の「ホームに戻る」ボタン。コーデ未適用なら確認を挟む
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/7
//------------------------------------------------------------------------------
//  それまで SceneChangeButton がそのままシーン遷移していたが、
//  プレビュー中の着せ替え(まだApplyOutfit()していない変更)を
//  気付かずに捨ててしまわないよう、未適用の時だけ確認ダイアログを挟む。
//==============================================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class DressUpHomeGuard : MonoBehaviour {
    [Header("戻り先シーン")]
    [SerializeField] private string sceneName = "CharaSelect";

    [Header("未適用の変更がある時に出す確認ダイアログ")]
    [SerializeField] private ConfirmDialog confirmDialog;
    [SerializeField] private string confirmMessage = "コーデを適用していません！\nこのまま戻りますか？";

    void Start() {
        GetComponent<Button>().onClick.AddListener(OnClickHome);
    }

    private void OnClickHome() {
        // 妖精の誕生フロー中(畑で生まれた子に名前を付けるまで)は、ここでは何もしない。
        // 同じボタンに NewFairyNamingPanel が「名前入力を開く」処理を登録していて、
        // 名前を付け終えるまでは名づけをすっ飛ばして街へ戻らせたくないため。
        if (FairyBirthFlow.IsNamingFlow) return;

        var character = DressUpTarget.Instance != null ? DressUpTarget.Instance.Current : null;
        bool dirty = character != null && character.HasUnsavedChanges;

        if (dirty && confirmDialog != null) {
            confirmDialog.Open(GoHome, confirmMessage);
            return;
        }

        GoHome();
    }

    private void GoHome() {
        if (string.IsNullOrEmpty(sceneName)) {
            Debug.LogWarning($"{name}: sceneName が設定されていません");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}
