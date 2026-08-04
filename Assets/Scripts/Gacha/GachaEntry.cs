//==============================================================================
//  File   : GachaEntry.cs
//  Brief  : ガチャの排出テーブル1件分(どのGameItemを・どのタブに出すか)
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/4
//------------------------------------------------------------------------------
//  アイテム本体の情報(名前・アイコン・説明・レアリティ)は GameItem 側が持つ。
//  category はガチャ画面のタブ(街装飾/服)の振り分け用。
//  (GameItem の実際の型が何であっても、どちらのタブに出すかはここで明示的に決める)
//==============================================================================
using UnityEngine;

[CreateAssetMenu(menuName = "Gacha/GachaEntry")]
public class GachaEntry : ScriptableObject {
    [Header("出るアイテム")]
    public GameItem item; // 着せ替え/街クリエイト/その他、どれでも可

    [Header("タブ振り分け")]
    public GachaCategory category;
}
