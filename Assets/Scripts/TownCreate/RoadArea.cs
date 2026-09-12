//==============================================================================
//  File   : RoadArea.cs
//  Brief  : 街クリエイトの中央広場・道の仮表示(参考画像の水色エリア)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  グリッド中心を基準にしたひし形(上下左右で半径を変えられる)の範囲を
//  水色に塗るだけの仮実装。本物の道タイル・当たり判定などはまだ無い。
//  FacilitySlotの上から塗りつぶさないよう、道→施設の順で適用すること。
//==============================================================================
using UnityEngine;

public class RoadArea : MonoBehaviour {
    [Header("中心からの半径(ワールド単位、上下左右で別々に指定できる)")]
    [SerializeField] private float halfLeft = 15f;
    [SerializeField] private float halfRight = 15f;
    [SerializeField] private float halfUp = 5f;
    [SerializeField] private float halfDown = 10f;

    [Header("見た目")]
    [SerializeField] private Color roadColor = new Color(0.55f, 0.75f, 0.92f, 1f);

    [ContextMenu("Apply")]
    public void Apply() {
        var grid = GetComponentInParent<IsoTownGrid>();
        if (grid == null) {
            Debug.LogWarning("[RoadArea] 親に IsoTownGrid が見つかりません: " + name);
            return;
        }

        var tiles = grid.GetComponentsInChildren<IsoTile>();
        Bounds? total = null;
        foreach (var tile in tiles) {
            var sr = tile.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            if (total == null) total = sr.bounds;
            else { var b = total.Value; b.Encapsulate(sr.bounds); total = b; }
        }
        if (total == null) return;
        Vector3 center = total.Value.center;

        foreach (var tile in tiles) {
            Vector3 p = tile.transform.position - center;
            float halfX = p.x >= 0f ? halfRight : halfLeft;
            float halfY = p.y >= 0f ? halfUp : halfDown;
            if (halfX <= 0f || halfY <= 0f) continue;

            bool inside = Mathf.Abs(p.x) / halfX + Mathf.Abs(p.y) / halfY <= 1f;
            if (inside) tile.SetHighlight(roadColor);
        }
    }
}
