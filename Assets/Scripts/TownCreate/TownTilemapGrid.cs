//==============================================================================
//  File   : TownTilemapGrid.cs
//  Brief  : 街クリエイトの地面グリッド(Tilemap版)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  GridのcellLayoutをIsometricにして、あらかじめ45度回転させておいたひし形の
//  スプライト(ground_diamond.png)を敷き詰める。見た目の「横2:縦1」比率は
//  このGameObject自体のScaleYを0.5にして作る(前段のIsoTownGridと同じ考え方)。
//  タイル自体は回転させていないので、SetCellRotationは装飾の90度回転など
//  今後の用途にそのまま使える。
//==============================================================================
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class TownTilemapGrid : MonoBehaviour {
    [Header("グリッドサイズ(タイル数)")]
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;

    [Header("参照")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase groundTile;

    [ContextMenu("Generate")]
    public void Generate() {
        if (tilemap == null || groundTile == null) {
            Debug.LogWarning("[TownTilemapGrid] tilemap / groundTile が未設定です");
            return;
        }

        tilemap.ClearAllTiles();
        transform.localScale = new Vector3(1f, 0.5f, 1f);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                tilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
            }
        }
    }

    /// <summary>指定セルの色を変える(道・施設のハイライト用)</summary>
    public void SetCellColor(int x, int y, Color color) {
        var pos = new Vector3Int(x, y, 0);
        tilemap.SetTileFlags(pos, TileFlags.None);
        tilemap.SetColor(pos, color);
        MarkTilemapDirty();
    }

    /// <summary>指定セルのタイルを追加で回転させる(装飾配置の90度回転などに使う想定)</summary>
    public void SetCellRotation(int x, int y, float extraDegrees) {
        var pos = new Vector3Int(x, y, 0);
        tilemap.SetTileFlags(pos, TileFlags.None);
        tilemap.SetTransformMatrix(pos, Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, extraDegrees)));
        MarkTilemapDirty();
    }

    // TilemapのSetColor/SetTransformMatrixは、明示的にDirty化しないとエディタ上での
    // 保存やPlayMode終了時の状態復元に反映されないことがあるため、変更のたびに呼ぶ。
    private void MarkTilemapDirty() {
#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(tilemap);
#endif
    }

    public Vector3 CellCenterWorld(int x, int y) {
        return tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));
    }
}
