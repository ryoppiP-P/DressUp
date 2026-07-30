//==============================================================================
//  File   : MissionData.cs
//  Brief  : ミッションデータの1件分の定義
// 
//  Author : Ryoto Kikuchi
//  Date   : 2026/7/18
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;

[CreateAssetMenu(menuName = "Mission/MissionData")]
public class MissionData : ScriptableObject {
    [Header("識別")]
    public string missionId;              // セーブのキー（不変・ユニーク）
    public MissionCategory category;      // Daily / Weekly / Challenge
    public MissionType type;              // 進捗カウント対象

    [Header("表示")]
    [TextArea] public string description; // 「街に装飾を置こう」など

    [Header("達成条件")]
    public int targetCount = 1;           // 目標回数（例：装飾を2個で 2）

    [Header("報酬")]
    public int rewardNut = 0;             // どんぐり
    public int rewardHoney = 0;           // はちみつ
}