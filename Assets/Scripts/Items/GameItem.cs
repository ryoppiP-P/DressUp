//==============================================================================
//  File   : GameItem.cs
//  Brief  : 全アイテム(着せ替え/街クリエイト/その他)共通の基底データ
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/4
//------------------------------------------------------------------------------
//  DressUpItem(着せ替え) / TownCreateItem(街クリエイト) / OtherItem(その他)は
//  すべてこれを継承する。Shop・Gachaはこの型への参照(+価格や確率などの文脈情報)
//  として扱い、アイテム本体の情報(名前・アイコン・レアリティ・説明)は
//  ここに一本化する。
//==============================================================================
using UnityEngine;

public abstract class GameItem : ScriptableObject {
    [Header("基本情報(全アイテム共通)")]
    public string itemName;
    public Sprite icon;
    public Rarity rarity;
    [TextArea] public string description;
}
