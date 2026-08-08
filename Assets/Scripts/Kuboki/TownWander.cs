//==============================================================================
//  File   : TownWander.cs
//  Brief  : キャラクターをランダムに建物間で巡回させるテスト用コントローラー
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/4
//------------------------------------------------------------------------------
//  シーン内の Building をランダムに選び、到着したら少し待って(仮の待機時間)
//  また別の建物へ向かう、を繰り返す。RouteTest.cs(単発テスト)の代わりに
//  常時動かしたいキャラクターに付ける。
//==============================================================================
using UnityEngine;

public class TownWander : MonoBehaviour {
    [Header("巡回させるキャラクター(空なら同じGameObjectから自動取得)")]
    [SerializeField] private CharacterManager character;

    [Header("到着後、次に出発するまでの待機時間(仮)")]
    [SerializeField] private float idleSecondsMin = 2f;
    [SerializeField] private float idleSecondsMax = 5f;

    private Building[] _allBuildings;
    private Building _currentTarget;
    private float _idleTimer;
    private bool _isIdling;

    void Awake() {
        if (character == null) character = GetComponent<CharacterManager>();
    }

    void Start() {
        _allBuildings = FindObjectsByType<Building>(FindObjectsSortMode.None);
        GoToRandomBuilding();
    }

    void Update() {
        if (character == null || _allBuildings == null || _allBuildings.Length == 0) return;

        if (_isIdling) {
            _idleTimer -= Time.deltaTime;
            if (_idleTimer <= 0f) {
                _isIdling = false;
                GoToRandomBuilding();
            }
            return;
        }

        // 移動中でなければ「到着した」とみなし、少し待ってから次の建物へ向かう
        if (!character.IsFollowingRoute) {
            _isIdling = true;
            _idleTimer = Random.Range(idleSecondsMin, idleSecondsMax);
        }
    }

    // 今いる建物とは別の建物をランダムに選んで移動を開始する
    private void GoToRandomBuilding() {
        Building next = _currentTarget;

        if (_allBuildings.Length > 1) {
            while (next == _currentTarget) {
                next = _allBuildings[Random.Range(0, _allBuildings.Length)];
            }
        } else {
            next = _allBuildings[0];
        }

        _currentTarget = next;
        character.StartRouteNavigation(next);
    }
}
