/*
* ファイル名　MapManager.cs
* タイトル　　マップ
* 作成者　　　久保木幹太
* 作成日     6月2日
* 更新日　　　6月2日
*/

using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("マップ内にあるすべての中継地点")]
    [SerializeField] private List<WayPoint> allWayPoints = new List<WayPoint>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            allWayPoints = new List<WayPoint>(FindObjectsByType<WayPoint>(FindObjectsSortMode.None));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public WayPoint GetNearestWayPoint(Vector3 position)
    {
        if (allWayPoints == null || allWayPoints.Count == 0) return null;

        WayPoint nearest = null;
        float minDistance = float.MaxValue;

        foreach (WayPoint wp in allWayPoints)
        {
            if (wp == null) continue;

            float dist = Vector2.Distance(position, wp.Position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = wp;
            }
        }

        return nearest;
    }

    public List<Vector3> CalculateShortestPath(Vector3 startPos, Vector3 targetPos)
    {
        WayPoint startWP = GetNearestWayPoint(startPos);
        WayPoint targetWP = GetNearestWayPoint(targetPos);

        if (startWP == null || targetWP == null) return new List<Vector3> { targetPos };

        List<WayPoint> openList = new List<WayPoint> { startWP };
        Dictionary<WayPoint, WayPoint> cameFrom = new Dictionary<WayPoint, WayPoint>();
        Dictionary<WayPoint, float> costSoFar = new Dictionary<WayPoint, float> { { startWP, 0 } };

        while (openList.Count > 0)
        {
            openList.Sort((a, b) => (costSoFar[a] + Vector2.Distance(a.Position, targetWP.Position)).CompareTo(costSoFar[b] + Vector2.Distance(b.Position, targetWP.Position)));
            WayPoint current = openList[0];
            openList.RemoveAt(0);

            if (current == targetWP) break;

            foreach (WayPoint next in current.neighbors)
            {
                float newCost = costSoFar[current] + Vector2.Distance(current.Position, next.Position);
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    cameFrom[next] = current;
                    openList.Add(next);
                }
            }
        }

        List<Vector3> finalRoute = new List<Vector3>();
        WayPoint temp = targetWP;
        while (temp != null)
        {
            finalRoute.Add(temp.Position);
            temp = cameFrom.ContainsKey(temp) ? cameFrom[temp] : null;
        }
        finalRoute.Reverse();
        finalRoute.Add(targetPos);
        return finalRoute;
    }
}