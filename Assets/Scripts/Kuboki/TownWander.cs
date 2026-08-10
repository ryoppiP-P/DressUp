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

    [Header("見た目(空なら自動取得。アニメ状態と向きの切り替えに使う)")]
    [SerializeField] private Character view;

    [Header("向きの反転")]
    [SerializeField] private bool autoFlip = true;
    [SerializeField] private float flipThreshold = 0.01f;   // 微小な揺れで反転しないための閾値

    private Building[] _allBuildings;
    private Building _currentTarget;
    private float _idleTimer;
    private bool _isIdling;
    private Vector3 _lastPos;

    void Awake() {
        if (character == null) character = GetComponent<CharacterManager>();
        if (view == null) view = GetComponentInChildren<Character>();
    }

    void Start() {
        _allBuildings = FindObjectsByType<Building>(FindObjectsSortMode.None);
        _lastPos = transform.position;
        GoToRandomBuilding();
    }

    void Update() {
        if (character == null || _allBuildings == null || _allBuildings.Length == 0) return;

        UpdateFacing();

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
            SetViewState(CharaState.Idle);
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
        SetViewState(CharaState.Walk);
    }

    // 実際の移動量から向きを決める（経路の曲がりにも自然に追従する）
    private void UpdateFacing() {
        if (!autoFlip || view == null) return;

        float dx = transform.position.x - _lastPos.x;
        _lastPos = transform.position;

        if (Mathf.Abs(dx) < flipThreshold) return;
        view.SetFacing(dx > 0f);
    }

    private void SetViewState(CharaState state) {
        if (view != null) view.SetState(state);
    }

}
