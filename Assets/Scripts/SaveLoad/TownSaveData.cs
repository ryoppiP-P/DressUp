//==============================================================================
//  File   : TownSaveData.cs
//  Brief  : 街にいるキャラクターの居場所のセーブデータ
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  SaveData.cs は Shift-JIS のため、構造はこの UTF-8 ファイル側に定義し、
//  SaveData 本体にはフィールドを1行足すだけにしている。
//==============================================================================
using System;
using System.Collections.Generic;

/// <summary>キャラ1体分の居場所(2Dなので x/y だけ持つ)</summary>
[Serializable]
public class CharacterPositionEntry {
    public string characterId;
    public float x;
    public float y;
}

[Serializable]
public class TownSaveData {
    public List<CharacterPositionEntry> positions = new List<CharacterPositionEntry>();
}
