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

    // 現在向かっている建物の名前(会話の{place}解決用)。移動中でなければ空文字。
    public string CurrentDestinationName => _currentTarget != null ? _currentTarget.BuildingName : "";

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

        // 建物の中で椅子などを回っている間はコルーチン側が動かす。
        // ドラッグでつかまれたら中断して、離された場所から普段の巡回に戻る。
        if (_activity != null) {
            if (character.IsHeld) AbortActivity();
            return;
        }

        // ドラッグでつかまれている間・会話中は、待機タイマーも進めず次の行き先も決めない
        if (character.IsPaused) return;

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
            // 中に座る場所がある建物なら、先にそこを回る(終わったらこの処理に戻ってくる)
            if (!_activityDone && _currentTarget != null && _currentTarget.ActivitySpots.Count > 0) {
                _activityDone = true;
                _activity = StartCoroutine(RunActivities(_currentTarget));
                return;
            }

            _isIdling = true;
            _idleTimer = Random.Range(idleSecondsMin, idleSecondsMax);
            SetViewState(CharaState.Idle);
        }
    }


    /// <summary>その場で立ち止まり、指定秒数待ってから(今いる場所から)また歩き出す</summary>
    public void HoldFor(float seconds) {
        AbortActivity();
        _isIdling = true;
        _idleTimer = seconds;
        SetViewState(CharaState.Idle);
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
        _activityDone = false;
        character.StartRouteNavigation(next);
        SetViewState(CharaState.Walk);
    }

    //--------------------------------------------------------------------------
    // 建物の中の椅子・座布団などを回る
    //--------------------------------------------------------------------------

    private Coroutine _activity;
    private bool _activityDone;
    private ActivitySpot _reservedSpot;

    /// <summary>
    /// order の小さい順に、空いている場所を1つずつ選んで歩いて行き、
    /// その場所に設定されたモーションを指定秒数ずつ再生する(会話などで止まっている間は時間を進めない)。
    /// 空きが無い order は飛ばす。全部終わったら普段の「少し待って次の建物へ」に戻る。
    /// </summary>
    private System.Collections.IEnumerator RunActivities(Building building) {
        var orders = new System.Collections.Generic.SortedSet<int>();
        foreach (var s in building.ActivitySpots) if (s != null) orders.Add(s.Order);

        foreach (int order in orders) {
            ActivitySpot spot = PickFreeSpot(building, order);
            if (spot == null) continue;
            _reservedSpot = spot;

            // 歩いて行く
            character.StartRouteTo(spot.Position);
            SetViewState(CharaState.Walk);
            yield return null;
            while (character.IsFollowingRoute || character.IsPaused) yield return null;

            // 座る位置に合わせる(足元の基準は歩く時と同じ)
            Vector3 seat = spot.Position;
            seat.y += character.RouteFeetOffsetY;
            seat += (Vector3)spot.SeatOffset;
            seat.z = transform.position.z;
            transform.position = seat;
            _lastPos = seat;
            if (view != null && spot.FaceTo != ActivitySpot.Facing.Keep) view.SetFacing(spot.FaceTo == ActivitySpot.Facing.Right);

            // モーションを順番に再生する
            foreach (var step in spot.Steps) {
                SetViewState(step.state);
                float t = step.seconds;
                while (t > 0f) {
                    if (!character.IsPaused) t -= Time.deltaTime;
                    yield return null;
                }
            }

            spot.Release(character);
            _reservedSpot = null;
        }

        _activity = null;
        // ここで Update が「到着済み・活動済み」と判断して、少し待ってから次の建物へ向かう
        SetViewState(CharaState.Idle);
    }

    private ActivitySpot PickFreeSpot(Building building, int order) {
        var free = new System.Collections.Generic.List<ActivitySpot>();
        foreach (var s in building.ActivitySpots)
            if (s != null && s.Order == order && s.IsFree) free.Add(s);
        if (free.Count == 0) return null;

        var picked = free[Random.Range(0, free.Count)];
        return picked.TryReserve(character) ? picked : null;
    }

    private void AbortActivity() {
        if (_activity != null) { StopCoroutine(_activity); _activity = null; }
        if (_reservedSpot != null) { _reservedSpot.Release(character); _reservedSpot = null; }
    }

    void OnDisable() {
        AbortActivity();
    }

    // 実際の移動量から向きを決める（経路の曲がりにも自然に追従する）
    private void UpdateFacing() {
        if (!autoFlip || view == null) return;

        float dx = transform.position.x - _lastPos.x;
        _lastPos = transform.position;

        // 会話中などの一時停止中は、テレポート的な位置補正で向きが乱れないよう向き更新をスキップする
        if (character != null && character.IsPaused) return;

        if (Mathf.Abs(dx) < flipThreshold) return;
        view.SetFacing(dx > 0f);
    }

    private void SetViewState(CharaState state) {
        if (view != null) view.SetState(state);
    }

}
