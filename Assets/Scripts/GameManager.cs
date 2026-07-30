/*
* ファイル名　GameManager.cs
* タイトル　　ゲームマネージャー（シーン遷移やミッションへの移動などやる）
* 作成者     久保木幹太 with 菊池凌斗
* 作成日     6月17日
* 更新日     6月17日
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ボタンを扱うために必要

public class GameManager : MonoBehaviour
{
    // ボタンが何をするか
    public enum ActionType {
        ChangeScene,   // シーン遷移
        OpenPanel,     // パネルを開く
        ClosePanel,    // パネルを閉じる
        TogglePanel,   // パネルの開閉を切り替え
    }

    // ボタンとシーン名のペアを定義する構造体
    [Serializable]
    public struct ButtonSetting
    {
        [Tooltip("設定するUIのボタンオブジェクト")]
        public Button buttonObject;

        [Tooltip("このボタンの動作")]
        public ActionType action;

        [Tooltip("ChangeScene のとき：遷移先シーン名")]
        [BuildScene]public string targetSceneName;

        [Tooltip("Open/Close/Toggle のとき：対象のパネル(GameObject)")]
        public GameObject targetPanel;
    }

    [Header("ボタンと遷移先シーンのリスト")]
    [SerializeField] private List<ButtonSetting> buttonSettingsList = new List<ButtonSetting>();

    void Start()
    {
        // リストが空なら何もしない
        if (buttonSettingsList == null) return;

        // リストに登録されたすべてのボタンに対して、クリック時のイベントを登録する
        foreach (ButtonSetting setting in buttonSettingsList) {
            if (setting.buttonObject == null) continue;

            // foreach のクロージャ対策でローカルにコピー
            ButtonSetting s = setting;

            switch (s.action) {
                case ActionType.ChangeScene:
                    if (string.IsNullOrEmpty(s.targetSceneName)) continue;
                    s.buttonObject.onClick.AddListener(() => ChangeScene(s.targetSceneName));
                    break;

                case ActionType.OpenPanel:
                    if (s.targetPanel == null) continue;
                    s.buttonObject.onClick.AddListener(() => SetPanel(s.targetPanel, true));
                    break;

                case ActionType.ClosePanel:
                    if (s.targetPanel == null) continue;
                    s.buttonObject.onClick.AddListener(() => SetPanel(s.targetPanel, false));
                    break;

                case ActionType.TogglePanel:
                    if (s.targetPanel == null) continue;
                    s.buttonObject.onClick.AddListener(() => TogglePanel(s.targetPanel));
                    break;
            }
        }
    }

    /// <summary>
    /// 指定されたシーン名に遷移する
    /// </summary>
    private void ChangeScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>パネルの表示/非表示を設定</summary>
    private void SetPanel(GameObject panel, bool open) {
        if (panel != null) panel.SetActive(open);
    }

    /// <summary>パネルの開閉を切り替え</summary>
    private void TogglePanel(GameObject panel) {
        if (panel != null) panel.SetActive(!panel.activeSelf);
    }
}