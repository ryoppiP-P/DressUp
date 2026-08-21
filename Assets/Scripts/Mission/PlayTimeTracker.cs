//==============================================================================
//  File   : PlayTimeTracker.cs
//  Brief  : 累計プレイ時間をセーブに積む
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/21
//------------------------------------------------------------------------------
//  「N時間プレイしよう」のミッション用。
//  SaveManager と同じ GameObject に付ける想定(あちらは DontDestroyOnLoad なので、
//  このスクリプトもシーンをまたいで生き残り、どの画面にいても時間を数えられる)。
//  毎フレーム保存すると重いので、一定間隔でだけ書き出す。
//==============================================================================
using UnityEngine;

public class PlayTimeTracker : MonoBehaviour {
    [Header("セーブに書き出す間隔(秒)")]
    [SerializeField] private float saveInterval = 30f;

    private float _sinceSave;

    void Update() {
        if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return;

        SaveManager.Instance.Current.missionData.playSeconds += Time.unscaledDeltaTime;

        _sinceSave += Time.unscaledDeltaTime;
        if (_sinceSave < saveInterval) return;

        _sinceSave = 0f;
        SaveManager.Instance.SaveAuto();
    }

    // アプリを閉じる/バックグラウンドに行く時に取りこぼさないようにする
    void OnApplicationPause(bool paused) {
        if (paused) Flush();
    }

    void OnApplicationQuit() {
        Flush();
    }

    private void Flush() {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.SaveAuto();
    }

    /// <summary>累計プレイ時間(分)</summary>
    public static int TotalMinutes {
        get {
            if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return 0;
            return Mathf.FloorToInt((float)(SaveManager.Instance.Current.missionData.playSeconds / 60.0));
        }
    }
}
