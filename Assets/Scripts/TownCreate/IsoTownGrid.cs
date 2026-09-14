//==============================================================================
//  File   : IsoTownGrid.cs
//  Brief  : 街クリエイトの配置可能グリッドの基盤(アイソメのひし形グリッドを生成する)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  作り方(依頼どおり):
//   ・親(このオブジェクト自身)の ScaleY を 0.5 にして全体を押しつぶす
//   ・タイル(子)は正方形のスプライトのまま RotationZ=45 で回転させる
//   ・タイルの座標は「普通の正方形グリッドを45度回転させた位置」に置く
//     (gx,gy) -> ((gx-gy)*d, (gx+gy)*d)  ※ d = タイル1枚の実サイズ / √2
//  この2つを組み合わせると、正方形を並べただけなのに継ぎ目なく
//  「横2:縦1」のアイソメ形のひし形グリッドになる。
//
//  装飾の配置・地面の編集禁止化などは後で追加する(今回はグリッドの基盤のみ)。
//==============================================================================
using System.Collections.Generic;
using UnityEngine;

public class IsoTownGrid : MonoBehaviour {
    [Header("グリッドサイズ(タイル数)")]
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;

    [Header("見た目(mapTileの地面用スプライト)")]
    [SerializeField] private Sprite groundSprite;

    [Header("タイルのプレハブ(正方形スプライト1枚)")]
    [SerializeField] private IsoTile tilePrefab;

    [Header("タイル同士の隙間(継ぎ目)対策の重なり倍率(1=ぴったり、1.02=2%だけ大きくして重ねる)")]
    [SerializeField] private float tileOverlapScale = 1.03f;

    private readonly List<IsoTile> _tiles = new List<IsoTile>();

    public IReadOnlyList<IsoTile> Tiles => _tiles;

    /// <summary>
    /// グリッド座標からタイルを探す(見つからなければnull)。
    /// _tiles はコンパイル(ドメインリロード)で消えるランタイム専用キャッシュなので、
    /// 実際のヒエラルキーを都度探す(生成直後に限らずいつ呼んでも正しく動くように)。
    /// </summary>
    public IsoTile GetTile(int gx, int gy) {
        var found = transform.Find("Tile_" + gx + "_" + gy);
        return found != null ? found.GetComponent<IsoTile>() : null;
    }

    /// <summary>1タイルの実サイズ(ワールド単位、正方形の1辺)</summary>
    public float TileWorldSize => groundSprite != null ? groundSprite.bounds.size.x : 0f;

    [ContextMenu("Generate Grid")]
    public void GenerateGrid() {
        Clear();

        if (groundSprite == null || tilePrefab == null) {
            Debug.LogWarning("[IsoTownGrid] groundSprite / tilePrefab が未設定です");
            return;
        }

        transform.localScale = new Vector3(1f, 0.5f, 1f);

        float tileSize = groundSprite.bounds.size.x;
        float d = tileSize / Mathf.Sqrt(2f);

        for (int gy = 0; gy < height; gy++) {
            for (int gx = 0; gx < width; gx++) {
                var tile = Instantiate(tilePrefab, transform);
                tile.name = "Tile_" + gx + "_" + gy;
                tile.transform.localPosition = new Vector3((gx - gy) * d, (gx + gy) * d, 0f);
                tile.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                tile.transform.localScale = Vector3.one * tileOverlapScale;
                // 奥(gx+gyが大きい=画面上で上側)を先に描き、手前を後で描く
                tile.Setup(gx, gy, groundSprite, -(gx + gy));
                _tiles.Add(tile);
            }
        }
    }

    [ContextMenu("Clear Grid")]
    public void Clear() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
        _tiles.Clear();
    }
}
