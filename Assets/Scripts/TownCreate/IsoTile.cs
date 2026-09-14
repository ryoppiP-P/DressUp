//==============================================================================
//  File   : IsoTile.cs
//  Brief  : 街クリエイトのグリッド1マス分(アイソメ表示用に45度回転させて使う)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/12
//------------------------------------------------------------------------------
//  今は見た目(地面のスプライト)だけ。配置可否のハイライトや装飾の紐付けは
//  配置モードを作る時にここへ足していく想定。
//==============================================================================
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IsoTile : MonoBehaviour {
    [SerializeField] private SpriteRenderer visual;

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    void Reset() {
        visual = GetComponent<SpriteRenderer>();
    }

    public void Setup(int gridX, int gridY, Sprite sprite, int sortingOrder) {
        GridX = gridX;
        GridY = gridY;

        if (visual == null) visual = GetComponent<SpriteRenderer>();
        visual.sprite = sprite;
        visual.sortingOrder = sortingOrder;
    }

    /// <summary>施設の予約スペースなどを示すため、タイルの色を変える(元に戻す時はColor.whiteを渡す)</summary>
    public void SetHighlight(Color color) {
        if (visual == null) visual = GetComponent<SpriteRenderer>();
        visual.color = color;
    }
}
