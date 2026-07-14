/*
* ファイル名 CharacterManager.cs
* タイトル   キャラクターマネージャー
* 作成者     久保木幹太
* 作成日     5月15日
* 更新日     6月15日（enum対応）
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

public class CharacterManager : MonoBehaviour
{
    [Header("特徴データ")]
    public List<CharaData> dataList = new List<CharaData>();

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

    // 全キャラを探すための静的リスト（登録/解除で管理）
    private static readonly List<CharacterManager> _all = new();

    private void OnEnable() { _all.Add(this); }
    private void OnDisable() { _all.Remove(this); }

    private void Update()
    {
        if (isFollowingRoute)
        {
            if (currentRoute == null || currentRoute.Count == 0) return;

            Vector3 targetPos = currentRoute[currentRouteIndex];
            targetPos.z = transform.position.z;

            // 目的地に向けて直線移動させる処理
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            CheckRouteProgress();
        }
        CheckInteractions();
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