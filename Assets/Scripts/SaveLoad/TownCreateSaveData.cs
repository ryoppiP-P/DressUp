//==============================================================================
//  File   : TownCreateSaveData.cs
//  Brief  : 街クリエイトで配置した装飾のセーブデータ
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/9/14
//------------------------------------------------------------------------------
//  SaveData.cs は Shift-JIS のため、構造はこの UTF-8 ファイル側に定義し、
//  SaveData 本体にはフィールドを1行足すだけにしている(TownSaveData.csと同じ方式)。
//  decorationId は配列の並び順(index)ではなく Tile アセット名で持つ。
//  将来装飾の並びが変わっても、既存のセーブデータが指す装飾がズレないようにするため。
//==============================================================================
using System;
using System.Collections.Generic;

/// <summary>装飾1個ぶんの配置情報</summary>
[Serializable]
public class PlacedDecorationEntry {
    public int x;
    public int y;
    public string decorationId; // 装飾Tileアセットの名前(例: "deco_mushroom_red")
}

[Serializable]
public class TownCreateSaveData {
    public List<PlacedDecorationEntry> placedDecorations = new List<PlacedDecorationEntry>();

    // 初期所持(装飾を各1個ずつ配る)を、既に行ったかどうか。
    // ConsumableBridgeの個数(GetCount<=0)では「配布済みだが配置して使い切った」と
    // 「まだ一度も配ってない」を区別できないため、専用のフラグで管理する。
    public bool starterDecorationsGranted = false;
}
