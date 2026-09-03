/*
* ファイル名 CharacterManager.cs
* タイトル   キャラクターマネージャー
* 作成者     久保木幹太
* 作成日     5月15日
* 更新日     8月4日（Ryoto Kikuchi：CharaId・すれ違い判定を追加）
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// キャラクターの構成要素
[Serializable]
public struct CharaData
{
    public PersonalityAxis AttributeType; // 列挙型の特徴
    [Range(0, 100)] public int Parameter;   // 特徴の値
}

// 親密度のInspector表示用。
// ここを直接書き換えても保存はされない。実データはSaveManager側(SaveData.intimacyData)にある。
[Serializable]
public struct IntimacyDisplay
{
    public string otherCharaId;      // 相手のcharaId
    [Range(0, 100)] public int Value; // 現在の親密度
}

public class CharacterManager : MonoBehaviour
{
    [Header("特徴データ")]
    public List<CharaData> dataList = new List<CharaData>();

    [Header("親密度(表示専用。ここを編集しても保存されません)")]
    [SerializeField] private List<IntimacyDisplay> intimacyList = new List<IntimacyDisplay>();

    private NavMeshAgent agent;

    private List<Vector3> currentRoute = new List<Vector3>();
    private int currentRouteIndex = 0;
    private bool isFollowingRoute = false;

    [Header("ルート移動・到着判定設定")]
    [Tooltip("次の中継地点にarrivalDistance分近づいたら到着とみなして次の中間地点に向かう")]
    [SerializeField] private float arrivalDistance = 0.2f;

    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 3.0f;

    [Header("交流設定")]
    [SerializeField] private float interactRange = 1.0f;      // この距離以内で交流
    [SerializeField] private float interactCooldown = 3.0f;   // 同じ相手と再交流するまでの間隔

    [Header("交流でのパラメータ変化")]
    [SerializeField] private int interactDelta = 2; // 1回の交流で動かす量

    // 相手ごとの「次に交流できる時刻」
    private readonly Dictionary<CharacterManager, float> _nextInteractTime = new();

    [Header("すれ違い判定(道で近づいたら止まって親密度アップ)")]
    [SerializeField] private float passByRange = 0.4f;        // これ以下の距離で「すれ違い」とみなす
    [SerializeField] private float passByPauseSeconds = 2f;   // 仮：すれ違ったら止まる秒数
    [SerializeField] private int passByIntimacyGain = 5;      // 仮：すれ違い1回あたりの親密度上昇量
    [SerializeField] private float passByCooldown = 5f;       // 同じ相手と連続ですれ違い判定しないための猶予

    // 一時停止の残り秒数(0より大きい間は移動しない)
    private float _pauseTimer = 0f;
    // 相手ごとの「次にすれ違い判定できる時刻」
    private readonly Dictionary<CharacterManager, float> _nextPassByTime = new();

    // 現在ルート移動中かどうか(到着したかの判定に外部から使う)
    public bool IsFollowingRoute => isFollowingRoute;

    // すれ違い会話などで一時停止中かどうか(会話中は向き・アニメーションを動かさないための判定に使う)
    public bool IsPaused => _pauseTimer > 0f;

    // 一時停止の残り秒数を外部(TalkManager)から設定する。
    // 今の残り時間より短い場合は縮めない(誘い中→会話本編で必要秒数が変わるため延長のみ)。
    public void SetPauseSeconds(float seconds) {
        _pauseTimer = Mathf.Max(_pauseTimer, seconds);
    }

    // このキャラクターのセーブキー(同じGameObjectのCharacterコンポーネントから取得)
    public string CharaId {
        get {
            var c = GetComponent<Character>();
            return c != null ? c.CharacterId : null;
        }
    }

    // 全キャラを探すための静的リスト（登録/解除で管理）
    private static readonly List<CharacterManager> _all = new();

    private void OnEnable() { _all.Add(this); }
    private void OnDisable() { _all.Remove(this); }

    private void Update()
    {
        if (_pauseTimer > 0f)
        {
            // すれ違いで一時停止中は移動しない
            _pauseTimer -= Time.deltaTime;
        }
        else if (isFollowingRoute)
        {
            if (currentRoute == null || currentRoute.Count == 0) return;

            Vector3 targetPos = currentRoute[currentRouteIndex];
            targetPos.z = transform.position.z;

            // 目的地に向けて直線移動させる処理
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            CheckRouteProgress();
        }
        CheckInteractions();
        CheckPassBy();
        RefreshIntimacyDisplay();
    }

    // 親密度のInspector表示(intimacyList)を、SaveManager側の実データから作り直す
    private void RefreshIntimacyDisplay() {
        if (SaveManager.Instance == null) return;

        string myId = CharaId;
        if (string.IsNullOrEmpty(myId)) return;

        intimacyList.Clear();
        foreach (var other in _all) {
            if (other == this) continue;

            string otherId = other.CharaId;
            if (string.IsNullOrEmpty(otherId)) continue;

            intimacyList.Add(new IntimacyDisplay {
                otherCharaId = otherId,
                Value = SaveManager.Instance.GetIntimacy(myId, otherId)
            });
        }
    }

    // 建物から歩行ルートを取得して最初の中継地点へ向けて移動開始する
    public void StartRouteNavigation(Building targetBuilding)
    {
        if (targetBuilding == null) return;

        // MapManagerに最短ルートを計算してもらう！
        currentRoute = MapManager.Instance.CalculateShortestPath(transform.position, targetBuilding.DestinationPosition);

        if (currentRoute != null && currentRoute.Count > 0)
        {
            currentRouteIndex = 0;
            isFollowingRoute = true;
        }
    }

    // 現在向かっている中継地点への到着をチェックし次第次の座標へ目的地を更新する
    private void CheckRouteProgress()
    {
        if (currentRoute == null || currentRoute.Count == 0) return;

        // 2Dで正確な直線距離を計算
        float distance = Vector2.Distance(transform.position, currentRoute[currentRouteIndex]);

        // 設定した到着判定距離以下になったら到着したとみなす
        if (distance <= arrivalDistance)
        {
            currentRouteIndex++;

            // まだルートの途中に座標が残っている場合
            if (currentRouteIndex < currentRoute.Count)
            {

            }
            else
            {
                isFollowingRoute = false;
                currentRoute.Clear();
            }
        }
    }

    // 近くのキャラと交流する
    private void CheckInteractions() {
        foreach (var other in _all) {
            if (other == this) continue;

            float dist = Vector2.Distance(transform.position, other.transform.position);
            if (dist > interactRange) continue;

            // クールダウン中ならスキップ
            if (_nextInteractTime.TryGetValue(other, out float next) && Time.time < next)
                continue;

            Interact(other);

            // 双方にクールダウンを設定（二重発生を防ぐ）
            _nextInteractTime[other] = Time.time + interactCooldown;
            other._nextInteractTime[this] = Time.time + interactCooldown;
        }
    }

    // 道ですれ違ったキャラクター同士を一時停止させ、親密度を上げる
    private void CheckPassBy() {
        if (!isFollowingRoute) return; // 移動中(道の上)でなければ判定しない
        if (IsPaused) return;          // 会話中に別の相手へ声をかけない

        string myId = CharaId;
        if (string.IsNullOrEmpty(myId)) return;

        foreach (var other in _all) {
            if (other == this) continue;
            if (!other.isFollowingRoute) continue;
            if (other.IsPaused) continue; // 相手が誰かと会話中なら割り込まない

            string otherId = other.CharaId;
            if (string.IsNullOrEmpty(otherId) || otherId == myId) continue;

            float dist = Vector2.Distance(transform.position, other.transform.position);
            if (dist > passByRange) continue;

            // クールダウン中ならスキップ
            if (_nextPassByTime.TryGetValue(other, out float next) && Time.time < next) continue;

            // すれ違い成立：お互い一時停止させ、親密度を上げる(すれ違いは一発イベントなので即保存)
            // 「！」ポップアップで誘えた場合は、タップ待ちの間だけ停止時間を延長する
            // (実際に会話が始まった時の延長は TalkManager 側が SetPauseSeconds で行う)
            float offerSeconds = TalkManager.Instance != null ? TalkManager.Instance.OfferConversation(this, other) : 0f;
            float pauseSeconds = Mathf.Max(passByPauseSeconds, offerSeconds);
            _pauseTimer = pauseSeconds;
            other._pauseTimer = pauseSeconds;

            if (SaveManager.Instance != null)
                SaveManager.Instance.AddIntimacy(myId, otherId, passByIntimacyGain);

            // 停止時間+クールダウン分は再判定しない(双方に設定して二重発生を防ぐ)
            float cooldownUntil = Time.time + passByPauseSeconds + passByCooldown;
            _nextPassByTime[other] = cooldownUntil;
            other._nextPassByTime[this] = cooldownUntil;
        }
    }

    private void Interact(CharacterManager other) {
        Debug.Log($"--- {name} が {other.name} と交流 ---");
        // 例：全軸について、相手の値に少し近づく（影響を受ける）
        foreach (PersonalityAxis axis in Enum.GetValues(typeof(PersonalityAxis))) {
            int mine = GetData(axis);
            int yours = other.GetData(axis);

            int delta = 0;
            if (yours > mine) delta = +interactDelta;
            else if (yours < mine) delta = -interactDelta;

            if (delta != 0) {
                AddData(axis, delta);
                // 変化があった軸だけ、変化前 → 変化後 を出す
                int after = Mathf.Clamp(GetData(axis), 0, 100);
                Debug.Log($"  {axis}: {mine} → {after} (相手 {yours}, {(delta > 0 ? "+" : "")}{delta})");
            }
        }

        ClampAll(); // 0-100 に収める
    }

    // 全パラメータを 0-100 に収める
    private void ClampAll() {
        for (int i = 0; i < dataList.Count; i++) {
            var d = dataList[i];
            d.Parameter = Mathf.Clamp(d.Parameter, 0, 100);
            dataList[i] = d;
        }
    }

    // 特徴を受け取る
    public int GetData(PersonalityAxis type)
    {
        foreach (var pair in dataList)
        {
            if (pair.AttributeType == type) return pair.Parameter;
        }
        Debug.LogWarning($"{type} というステータスは見つかりませんでした");

        return 0;
    }

    // 特徴の値をセットする
    public void SetData(PersonalityAxis type, int newValue)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i].AttributeType == type)
            {
                // 構造体の値を更新してリストに戻す
                CharaData updatedPair = dataList[i];
                updatedPair.Parameter = newValue;
                dataList[i] = updatedPair;
                return;
            }
        }
        Debug.LogWarning($"{type} が見つからないため更新できませんでした");
    }

    // 特徴を加算(減算)
    public void AddData(PersonalityAxis type, int addValue)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i].AttributeType == type)
            {
                CharaData updatedPair = dataList[i];
                updatedPair.Parameter += addValue;
                dataList[i] = updatedPair;
                return;
            }
        }
        Debug.LogWarning($"{type} が見つからないため更新できませんでした");
    }
}