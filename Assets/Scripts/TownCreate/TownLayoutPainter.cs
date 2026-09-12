//==============================================================================
//  File   : TownLayoutPainter.cs
//  Brief  : 施設スペースの状態を管理する(見た目の色付けは撤回済み)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  Tilemapの色(SetColor)はPlayModeの開始/終了をまたぐと保存されていても
//  復元されないことがある(Tilemap特有の癖)。[ExecuteAlways]でEdit modeでも
//  Awakeを動かし、常にその場で塗り直す方式にすることで、保存状態に頼らず
//  常に正しい見た目にする。
//
//  「配置不可」の判定そのものはBlockedレイヤー(Tile Paletteでユーザーが直接
//  ペイントする)が持つ。施設スペースのサンド色ハイライトは、Blockedレイヤーの
//  赤ハッチングと重なって濃く見える不具合が出たため、ユーザーの指示で撤回済み
//  (道の色付けも同様の理由で既に撤回済み)。
//==============================================================================
using UnityEngine;

[ExecuteAlways]
public class TownLayoutPainter : MonoBehaviour {
    [SerializeField] private TownTilemapGrid grid;

    [System.Serializable]
    public class FacilitySpot {
        public string name;
        public int originX;
        public int originY;
        public int sizeX = 3;
        public int sizeY = 3;
    }
    [SerializeField] private FacilitySpot[] facilities;

    void Awake() {
        Repaint();
    }

    /// <summary>色付けは撤回済みなので、白(元の草の色)に戻すだけ</summary>
    [ContextMenu("Repaint")]
    public void Repaint() {
        if (grid == null || facilities == null) return;

        foreach (var f in facilities) {
            for (int dy = 0; dy < f.sizeY; dy++) {
                for (int dx = 0; dx < f.sizeX; dx++) {
                    grid.SetCellColor(f.originX + dx, f.originY + dy, Color.white);
                }
            }
        }
    }
}
