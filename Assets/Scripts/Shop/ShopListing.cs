//==============================================================================
//  File   : ShopListing.cs
//  Brief  : ショップの出品情報(どのGameItemを・いくらで売るか)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/4
//------------------------------------------------------------------------------
//  アイテム本体の情報(名前・アイコン・説明・レアリティ)は GameItem 側が持つ。
//  ここでは「売り方」の情報(価格・通貨種別)だけを持つ。
//==============================================================================
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/ShopListing")]
public class ShopListing : ScriptableObject {
    [Header("売るアイテム")]
    public GameItem item; // 着せ替え/街クリエイト/その他、どれでも可

    [Header("価格")]
    public int price = 0;
    public CurrencyType currencyType;
}
