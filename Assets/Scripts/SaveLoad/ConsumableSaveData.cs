//==============================================================================
//  File   : ConsumableSaveData.cs
//  Brief  : 使うと減るアイテム(種・時短の実)の所持数
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/23
//------------------------------------------------------------------------------
//  既存の ItemSaveData.ownedItemIds は「持っているかどうか」の1ビットしか無く、
//  服やアクセのように一度手に入れたら消えないものが対象。
//  種や時短の実は使うと減るので、こちらで個数を持つ。
//==============================================================================
using System;
using System.Collections.Generic;

[Serializable]
public class ConsumableEntry {
    public string itemId;
    public int count;
}

[Serializable]
public class ConsumableSaveData {
    public List<ConsumableEntry> items = new List<ConsumableEntry> {
        // 初期所持アイテム
        new ConsumableEntry {itemId = "FairySeed_01", count = 1},
        //new ConsumableEntry {itemId = "TimeReduceItem_01", count = 1},
    };
}
