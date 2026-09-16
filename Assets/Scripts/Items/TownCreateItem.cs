//==============================================================================
//  File   : TownCreateItem.cs
//  Brief  : 街クリエイト用アイテム(街に配置する装飾アイテム)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/4
//------------------------------------------------------------------------------
//  所持数はConsumableBridge(itemId基準)で管理する(種・時短の実と同じ方式)。
//  ガチャ/ショップで手に入れると個数が増え、街クリエイトで配置すると1個減り、
//  撤去すると1個戻る。
//==============================================================================
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Items/TownCreateItem")]
public class TownCreateItem : GameItem {
    [Header("街クリエイトでの配置に使うTile")]
    public TileBase decorationTile;
}
