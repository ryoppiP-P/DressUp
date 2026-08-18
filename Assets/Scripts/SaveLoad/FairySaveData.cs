//==============================================================================
//  File   : FairySaveData.cs
//  Brief  : 妖精の育成(種スロット)と、生まれた妖精の名簿のセーブデータ
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  SaveData.cs は Shift-JIS のため、追加するデータ構造はこの UTF-8 ファイル側に
//  定義し、SaveData 本体にはフィールドを1行足すだけにしている。
//==============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 6軸の性格パラメータ(PersonalityAxis と対応)。
/// JsonUtility は Dictionary を保存できないため、素の int フィールドで持つ。
/// </summary>
[Serializable]
public class PersonalitySnapshot {
    public int mystery = 50;
    public int lonely = 50;
    public int shy = 50;
    public int caring = 50;
    public int whimsy = 50;
    public int spoil = 50;

    public int Get(PersonalityAxis axis) {
        switch (axis) {
            case PersonalityAxis.Mystery: return mystery;
            case PersonalityAxis.Lonely: return lonely;
            case PersonalityAxis.Shy: return shy;
            case PersonalityAxis.Caring: return caring;
            case PersonalityAxis.Whimsy: return whimsy;
            case PersonalityAxis.Spoil: return spoil;
        }
        return 0;
    }

    public void Set(PersonalityAxis axis, int value) {
        value = Mathf.Clamp(value, 0, 100);
        switch (axis) {
            case PersonalityAxis.Mystery: mystery = value; break;
            case PersonalityAxis.Lonely: lonely = value; break;
            case PersonalityAxis.Shy: shy = value; break;
            case PersonalityAxis.Caring: caring = value; break;
            case PersonalityAxis.Whimsy: whimsy = value; break;
            case PersonalityAxis.Spoil: spoil = value; break;
        }
    }

    public void Add(PersonalityAxis axis, int delta) {
        Set(axis, Get(axis) + delta);
    }
}

/// <summary>畑の種スロット1つ分。植えていない時は isPlanted = false。</summary>
[Serializable]
public class SeedSlotData {
    public bool isPlanted;

    // 植えた時刻(ISO 8601 / UTC)。実時間で育てるのでゲーム内時間ではなくこれを使う。
    public string plantedAtUtc = "";

    public float growSeconds;      // 育ちきるまでに必要な秒数
    public float reducedSeconds;   // 時短アイテムで縮めた合計秒数

    public List<string> keywords = new List<string>();          // 願いを込めた時に選んだキーワード
    public PersonalitySnapshot personality = new PersonalitySnapshot(); // キーワードから決まった性格
}

/// <summary>生まれた妖精1体分の記録。</summary>
[Serializable]
public class FairyRosterEntry {
    public string characterId;
    public string bornAtUtc = "";
    public PersonalitySnapshot personality = new PersonalitySnapshot();

    // 名前を付け終わったか。false の間は誕生フローの途中なので街には出さない。
    public bool namingDone;
}

[Serializable]
public class FairySaveData {
    public List<SeedSlotData> slots = new List<SeedSlotData>();
    public List<FairyRosterEntry> roster = new List<FairyRosterEntry>();

    // 次に生まれる妖精に振る番号。charaID_0001から。
    public int nextCharaNumber = 1;
}
