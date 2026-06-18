/*
* ファイル名　SceneChangeBotton.cs
* タイトル　　ボタン対応シーン変更
* 作成者     久保木幹太
* 作成日     6月17日
* 更新日     6月17日
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ボタンを扱うために必要

public class SceneChangeBotton : MonoBehaviour
{
    // ボタンとシーン名のペアを定義する構造体
    [Serializable]
    public struct ButtonSetting
    {
        [Tooltip("設定するUIのボタンオブジェクト")]
        public Button buttonObject;

        [Tooltip("このボタンを押したときに遷移するシーン名")]
        public string targetSceneName;
    }

    [Header("ボタンと遷移先シーンのリスト")]
    [SerializeField] private List<ButtonSetting> buttonSettingsList = new List<ButtonSetting>();

    void Start()
    {
        // リストが空なら何もしない
        if (buttonSettingsList == null) return;

        // リストに登録されたすべてのボタンに対して、クリック時のイベントを登録する
        foreach (ButtonSetting setting in buttonSettingsList)
        {
            // ボタンやシーン名が未設定のものはスキップ
            if (setting.buttonObject == null || string.IsNullOrEmpty(setting.targetSceneName))
            {
                continue;
            }

            // ボタンが押されたら、その設定のシーン名を引数にして関数を呼ぶ
            setting.buttonObject.onClick.AddListener(() => ChangeScene(setting.targetSceneName));
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
        else
        {

        }
    }
}