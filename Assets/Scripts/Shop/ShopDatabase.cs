//==============================================================================
//  File   : ShopDatabase.cs
//  Brief  : ショップアイテムの一覧を持つデータベース
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//  ItemDatabase(DressUp側)と同じ構成方針。
//  現時点では listings は空の想定(アイテムは別途作成中のため)。
//==============================================================================
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Shop/ShopDatabase")]
public class ShopDatabase : ScriptableObject {
    public List<ShopListing> listings;
}
