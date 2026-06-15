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

public enum CharaAttributeType
{
    [InspectorName("知性")] Intelligence,
    [InspectorName("優しさ")] Kindness,
    [InspectorName("活発")] Active,
    [InspectorName("社交性")] Sociability,
    [InspectorName("内向的")] Introverted,
    [InspectorName("わがまま")] Selfish,
}

// キャラクターの構成要素
[Serializable]
public struct CharaData
{
    public CharaAttributeType AttributeType; // 列挙型の特徴
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

    private void Awake()
    {
    }

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

    // 特徴を受け取る
    public int GetData(CharaAttributeType type)
    {
        foreach (var pair in dataList)
        {
            if (pair.AttributeType == type) return pair.Parameter;
        }
        Debug.LogWarning($"{type} というステータスは見つかりませんでした");

        return 0;
    }

    // 特徴の値をセットする
    public void SetData(CharaAttributeType type, int newValue)
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
    public void AddData(CharaAttributeType type, int addValue)
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