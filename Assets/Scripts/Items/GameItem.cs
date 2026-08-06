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
    [Header("識別")]
    // セーブデータ上のキー。所持アイテムはこの文字列で記録されるため、
    // 一度決めたら変更しないこと(変更すると所持済み判定が外れる)。
    // Mission 側の missionId と同じ扱い。
    public string itemId;

    [Header("基本情報(全アイテム共通)")]
    public string itemName;
    public Sprite icon;
    public Rarity rarity;
    [TextArea] public string description;

    [Header("所持")]
    // 最初から持っているアイテムか。
    // ガチャやショップで手に入れなくても最初から使えるもの(初期衣装など)は ON にする。
    // ON のアイテムはセーブに記録しなくても所持済みとして扱われる。
    public bool ownedByDefault;
}
