/*
* ファイル名　WayPoint.cs
* タイトル　　道の中間地点
* 作成者　　　久保木幹太
* 作成日     6月2日
* 更新日　　　6月2日
*/

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WayPoint : MonoBehaviour
{
    [Header("自動接続設定")]
    [SerializeField] private float connectRadius = 100.0f;
    [SerializeField] private int maxNearestPoints = 2;

    [Header("手動で繋ぐポイント")]
    public List<WayPoint> manualNeighbors = new List<WayPoint>();

    // 自動＋手動を合わせた最終的な隣接リスト
    public List<WayPoint> neighbors => GetCombinedNeighbors();

    public Vector3 Position => transform.position;

    // 自動と手動を統合する処理
    private List<WayPoint> GetCombinedNeighbors()
    { // 加筆しました
        List<WayPoint> all = new List<WayPoint>(manualNeighbors);
        var allWayPoints = FindObjectsByType<WayPoint>(FindObjectsSortMode.None);
        var nearby = allWayPoints.Where(wp => wp != this && Vector2.Distance(transform.position, wp.Position) <= connectRadius)
                                 .OrderBy(wp => Vector2.Distance(transform.position, wp.Position))
                                 .Take(maxNearestPoints);
        foreach (var wp in nearby) if (!all.Contains(wp)) all.Add(wp);
        return all;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.15f);

        foreach (WayPoint neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
#endif
}