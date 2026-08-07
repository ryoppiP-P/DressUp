//==============================================================================
//  File   : OtherItem.cs
//  Brief  : 着せ替え/街クリエイトに当てはまらないその他アイテム
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/4
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;

public enum OtherItemType
{
    TimeReduce, // 時間短縮アイテム
}

[CreateAssetMenu(menuName = "Items/OtherItem")]
public class OtherItem : GameItem 
{
    [Header("その他アイテム固有情報")]
    public OtherItemType itemType;

    public virtual bool Use(GameObject target)
    {
        Debug.Log($"{itemName} を使用しました");
        return true;
    }
}
