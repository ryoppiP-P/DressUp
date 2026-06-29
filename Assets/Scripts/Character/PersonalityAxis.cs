//==============================================================================
//  File   : PersonalityAxis.cs
//  Brief  : プレイヤーの性格軸定義
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/6/25
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;

public enum PersonalityAxis {
    [InspectorName("ふしぎ")] Mystery,
    [InspectorName("さみしがり")] Lonely,
    [InspectorName("てれや")] Shy,
    [InspectorName("めんどうみ")] Caring,
    [InspectorName("きまぐれ")] Whimsy,
    [InspectorName("あまえ")] Spoil,
}