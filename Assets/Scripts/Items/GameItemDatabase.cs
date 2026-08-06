//==============================================================================
//  File   : GameItemDatabase.cs
//  Brief  : GameItem をまとめて持つ汎用データベース
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/4
//------------------------------------------------------------------------------
//  着せ替えアイテムは既存の ItemDatabase(DressUpItem 専用)を使うため、
//  こちらは街クリエイト / その他アイテム用のデータベースとして使う。
//  用途ごとに .asset を分けて作る想定(例: TownCreateItemDatabase.asset)。
//==============================================================================
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/GameItemDatabase")]
public class GameItemDatabase : ScriptableObject {
    public List<GameItem> allItems = new List<GameItem>();
}
