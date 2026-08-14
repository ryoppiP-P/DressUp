//==============================================================================
//  File   : TownPositionSaver.cs
//  Brief  : 街のキャラクターを配置し、居場所を定期的にセーブする
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  ・シーンに最初から置かれているキャラは Awake で配置し直す
//    (TownWander が Start で経路を引くより先に済ませたいため)
//  ・名簿の妖精の生成はこの Start から呼ぶ。生成直後に配置されるので、
//    そちらも TownWander の Start より先になる
//  ・一定間隔でまとめてセーブする(毎フレーム書き込むと重いため)
//==============================================================================
using UnityEngine;

public class TownPositionSaver : MonoBehaviour {
    [Header("名簿から妖精を出すスポナー(ここから生成させる)")]
    [SerializeField] private TownFairySpawner spawner;

    [Header("居場所をセーブする間隔(秒)")]
    [SerializeField] private float saveInterval = 5f;

    private float _timer;

    void Awake() {
        // シーンに元から居るキャラを、前回の場所 or ランダムな場所へ
        foreach (var character in FindObjectsByType<Character>(FindObjectsSortMode.None))
            TownCharacterPlacement.Place(character);
    }

    void Start() {
        // 名簿の妖精を生成する(生成側で配置まで行う)
        if (spawner != null) spawner.SpawnAll();

        _timer = saveInterval;
    }

    void Update() {
        Tick(Time.deltaTime);
    }

    // テストから「N秒経った」を再現できるよう dt を受け取れる形にしてある
    public void Tick(float dt) {
        if (saveInterval <= 0f) return;

        _timer -= dt;
        if (_timer > 0f) return;

        _timer = saveInterval;
        SaveNow();
    }

    /// <summary>今いる場所を全員分まとめてセーブする</summary>
    public void SaveNow() {
        if (SaveManager.Instance == null) return;

        int count = 0;
        foreach (var character in FindObjectsByType<Character>(FindObjectsSortMode.None)) {
            if (character == null || string.IsNullOrEmpty(character.CharacterId)) continue;

            TownSaveBridge.SetPosition(character.CharacterId, character.transform.position);
            count++;
        }

        if (count > 0) TownSaveBridge.Flush();
    }

    // アプリを閉じる/バックグラウンドに回る時も取りこぼさないようにする
    void OnApplicationPause(bool paused) {
        if (paused) SaveNow();
    }

    void OnApplicationQuit() {
        SaveNow();
    }
}
