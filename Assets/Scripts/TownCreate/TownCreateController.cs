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
//
//  装飾はTownCreateItem(GameItem)として持ち、所持数はConsumableBridge(itemId基準)で
//  管理する(種・時短の実と同じ方式)。ガチャ/ショップで増え、配置すると1個減り、
//  撤去すると1個戻る。初回起動時だけ、装飾を1個ずつ配る。
//
//  配置した装飾の「どこに何を置いたか」はSaveManager(SaveData.townCreateData)へ
//  別途保存し、起動時に読み込んでDecorationsレイヤーへ復元する
//  (所持数の増減とは別の情報。置いた時点で所持数は既に1個消費済み)。
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

    [Header("配置できる装飾(ガチャ/ショップで手に入るTownCreateItem)")]
    [SerializeField] private TownCreateItem[] decorationItems;

    private TownEditMode _mode = TownEditMode.None;
    private int _selectedDecoration = 0;

    public TownEditMode Mode => _mode;
    public int SelectedDecoration => _selectedDecoration;
    public TownCreateItem[] DecorationItems => decorationItems;

    /// <summary>装飾を置く/撤去して所持数が変わった時に呼ばれる(UIの個数表示更新用)</summary>
    public event System.Action OnDecorationCountChanged;

    /// <summary>街クリ編集画面が開いている間はtrue。会話などの通常の街イベントを止めるのに使う。</summary>
    public static bool IsEditScreenOpen { get; set; }

    public void SetMode(TownEditMode mode) {
        _mode = mode;
    }

    public void SelectDecoration(int index) {
        if (decorationItems == null || index < 0 || index >= decorationItems.Length) return;
        _selectedDecoration = index;
    }

    void Start() {
        GrantStarterDecorationsIfNeeded();
        LoadPlacedDecorations();
    }

    /// <summary>初回だけ、街クリの装飾を1個ずつ配る</summary>
    private void GrantStarterDecorationsIfNeeded() {
        if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return;
        var data = SaveManager.Instance.Current.townCreateData;
        if (data.starterDecorationsGranted) return;
        if (decorationItems == null) return;

        foreach (var item in decorationItems) {
            if (item == null) continue;
            ConsumableBridge.Add(item.itemId, 1);
        }

        data.starterDecorationsGranted = true;
        SaveManager.Instance.SaveAuto();
    }

    /// <summary>セーブされている装飾を、起動時にDecorationsレイヤーへ復元する</summary>
    private void LoadPlacedDecorations() {
        if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return;
        var entries = SaveManager.Instance.Current.townCreateData.placedDecorations;

        foreach (var entry in entries) {
            TileBase tile = FindDecorationTile(entry.decorationId);
            if (tile == null) continue; // 見つからない装飾(削除済み等)は無視する

            var cell = new Vector3Int(entry.x, entry.y, 0);
            PlaceTileVisual(cell, tile);
        }
    }

    private TownCreateItem FindDecorationItem(string decorationId) {
        if (decorationItems == null) return null;
        foreach (var item in decorationItems) {
            if (item != null && item.itemId == decorationId) return item;
        }
        return null;
    }

    private TileBase FindDecorationTile(string decorationId) {
        var item = FindDecorationItem(decorationId);
        return item != null ? item.decorationTile : null;
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
        if (decorationItems == null || _selectedDecoration < 0 || _selectedDecoration >= decorationItems.Length) return;

        TownCreateItem item = decorationItems[_selectedDecoration];
        if (item == null || item.decorationTile == null) return;

        if (!ConsumableBridge.TryConsume(item.itemId, 1)) {
            Debug.Log("[TownCreate] " + item.itemName + " を持っていません");
            return;
        }

        PlaceTileVisual(cell, item.decorationTile);
        SaveDecoration(cell, item.itemId);
        OnDecorationCountChanged?.Invoke();

        // ミッション「街に装飾を置こう」
        if (MissionManager.Instance != null)
            MissionManager.Instance.Report(MissionType.PlaceDecoration, 1);
    }

    private void TryDelete(Vector3Int cell) {
        string decorationId = FindPlacedDecorationId(cell);
        if (decorationTilemap.GetTile(cell) == null) return;

        decorationTilemap.SetTile(cell, null);
        RemoveSavedDecoration(cell);

        if (!string.IsNullOrEmpty(decorationId)) ConsumableBridge.Add(decorationId, 1); // 撤去したら所持数へ戻す
        OnDecorationCountChanged?.Invoke();
    }

    /// <summary>Decorationsレイヤーへ実際にタイルを置く見た目の処理だけを行う(セーブはしない)</summary>
    private void PlaceTileVisual(Vector3Int cell, TileBase tile) {
        decorationTilemap.SetTile(cell, tile);
        decorationTilemap.SetTileFlags(cell, TileFlags.None);
        // 親(Grid)のScaleY=0.5を打ち消して、装飾が縦に潰れないようにする
        decorationTilemap.SetTransformMatrix(cell, Matrix4x4.Scale(new Vector3(1f, 2f, 1f)));
    }

    private string FindPlacedDecorationId(Vector3Int cell) {
        if (SaveManager.Instance == null) return null;
        var entries = SaveManager.Instance.Current.townCreateData.placedDecorations;
        var existing = entries.Find(e => e.x == cell.x && e.y == cell.y);
        return existing != null ? existing.decorationId : null;
    }

    private void SaveDecoration(Vector3Int cell, string decorationId) {
        if (SaveManager.Instance == null) return;
        var entries = SaveManager.Instance.Current.townCreateData.placedDecorations;

        var existing = entries.Find(e => e.x == cell.x && e.y == cell.y);
        if (existing != null) existing.decorationId = decorationId;
        else entries.Add(new PlacedDecorationEntry { x = cell.x, y = cell.y, decorationId = decorationId });

        SaveManager.Instance.SaveAuto();
    }

    private void RemoveSavedDecoration(Vector3Int cell) {
        if (SaveManager.Instance == null) return;
        var entries = SaveManager.Instance.Current.townCreateData.placedDecorations;
        entries.RemoveAll(e => e.x == cell.x && e.y == cell.y);
        SaveManager.Instance.SaveAuto();
    }
}
