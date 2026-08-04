//==============================================================================
//  File   : GachaDatabase.cs
//  Brief  : ガチャで出るアイテムの一覧を持つデータベース
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/3
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Gacha/GachaDatabase")]
public class GachaDatabase : ScriptableObject {
    public List<GachaEntry> entries;
}
