//==============================================================================
//  File   : SceneBgm.cs
//  Brief  : シーンに置くだけで、開始時に指定BGMを流す小さいヘルパー
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/10
//------------------------------------------------------------------------------
//  各シーンの適当なGameObject(Canvasなど)に付けて、bgm を選ぶだけ。
//  AudioManager が居なければ何もしない(次のシーンで居れば流れる)。
//==============================================================================
using UnityEngine;

public class SceneBgm : MonoBehaviour {
    [SerializeField] private BGMType bgm = BGMType.None;

    [Tooltip("同じBGMが既に流れている時は流し直さない(AudioManager側で判定済みだが明示)")]
    [SerializeField] private bool playOnStart = true;

    void Start() {
        if (playOnStart) Play();
    }

    public void Play() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(bgm);
    }
}
