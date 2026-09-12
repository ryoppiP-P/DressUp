//==============================================================================
//  File   : TownCreateController.cs
//  Brief  : 街クリエイトの配置モード/削除モード
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  配置モード: 選んだ装飾を、純粋な緑タイル(Blockedレイヤーに何も無い場所)にだけ置ける。
//  削除モード: 置いてある装飾をタップして消せる。
//  地面(Ground)には一切触らない(地面の編集はスコープ外)。
//
//  「配置不可」の場所はTile Palette経由でBlockedレイヤーに直接ペイントして
//  ユーザー自身が決められるようにしてある(コード側で自動計算しない)。
//==============================================================================
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum TownEditMode { None, Place, Delete }

public class TownCreateController : MonoBehaviour {
    [Header("参照")]
    [SerializeField] private Camera mapCamera;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap blockedTilemap; // ここに何かタイルがあれば配置不可(Tile Paletteで手動編集する)
    [SerializeField] private Tilemap decorationTilemap;
    [SerializeField] private TileBase pureGroundTile; // これ以外(土など)には置けない

    [Header("配置できる装飾")]
    [SerializeField] private TileBase[] decorationTiles;

    private TownEditMode _mode = TownEditMode.None;
    private int _selectedDecoration = 0;

    public TownEditMode Mode => _mode;
    public int SelectedDecoration => _selectedDecoration;

    public void SetMode(TownEditMode mode) {
        _mode = mode;
    }

    public void SelectDecoration(int index) {
        if (decorationTiles == null || index < 0 || index >= decorationTiles.Length) return;
        _selectedDecoration = index;
    }

    void Update() {
        if (_mode == TownEditMode.None) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 screenPos;
        if (Input.GetMouseButtonDown(0)) {
            screenPos = Input.mousePosition;
        } else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            screenPos = Input.GetTouch(0).position;
        } else {
            return;
        }

        float depth = -mapCamera.transform.position.z;
        Vector3 world = mapCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
        Vector3Int cell = groundTilemap.WorldToCell(world);

        if (_mode == TownEditMode.Place) TryPlace(cell);
        else if (_mode == TownEditMode.Delete) TryDelete(cell);
    }

    /// <summary>素の草タイルで、かつBlockedレイヤーに何も無い(配置可能な)場所かどうか</summary>
    public bool IsPureGreen(Vector3Int cell) {
        if (groundTilemap.GetTile(cell) != pureGroundTile) return false;
        if (blockedTilemap != null && blockedTilemap.GetTile(cell) != null) return false;
        return true;
    }

    private void TryPlace(Vector3Int cell) {
        if (!IsPureGreen(cell)) {
            Debug.Log("[TownCreate] ここには置けません(純粋な緑タイルのみ): " + cell);
            return;
        }
        if (decorationTilemap.GetTile(cell) != null) {
            Debug.Log("[TownCreate] すでに装飾があります: " + cell);
            return;
        }
        if (decorationTiles == null || _selectedDecoration < 0 || _selectedDecoration >= decorationTiles.Length) return;

        decorationTilemap.SetTile(cell, decorationTiles[_selectedDecoration]);
        decorationTilemap.SetTileFlags(cell, TileFlags.None);
        // 親(Grid)のScaleY=0.5を打ち消して、装飾が縦に潰れないようにする
        decorationTilemap.SetTransformMatrix(cell, Matrix4x4.Scale(new Vector3(1f, 2f, 1f)));
    }

    private void TryDelete(Vector3Int cell) {
        if (decorationTilemap.GetTile(cell) == null) return;
        decorationTilemap.SetTile(cell, null);
    }
}
