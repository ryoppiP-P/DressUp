//==============================================================================
//  File   : FairyKeywordTable.cs
//  Brief  : 選んだキーワードから妖精の性格(6軸)を組み立てる
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  増減値は ToggleSelectCheck.cs の調整をそのまま移植したもの。
//  ToggleSelectCheck 側は「トグルを触るたびに加算/減算していく」作りで
//  途中経過に左右されるため、確定用のこちらは
//  「全軸50スタート + 選ばれた3つ分を足す」という計算し直しの形にしている。
//
//  ToggleSelectCheck の Mom / Whim / Spoiled は
//  PersonalityAxis の Caring / Whimsy / Spoil に対応する。
//==============================================================================
using System;
using System.Collections.Generic;

public static class FairyKeywordTable {
    /// <summary>どの軸も最初はこの値から始まる</summary>
    public const int BaseValue = 50;

    public struct AxisDelta {
        public PersonalityAxis axis;
        public int value;
        public AxisDelta(PersonalityAxis axis, int value) { this.axis = axis; this.value = value; }
    }

    // キーワード名(トグルのGameObject名)→ 各軸の増減
    private static readonly Dictionary<string, AxisDelta[]> _table = new Dictionary<string, AxisDelta[]> {
        { "ふわふわ↑", new[] {
            new AxisDelta(PersonalityAxis.Mystery, +10),
            new AxisDelta(PersonalityAxis.Lonely,  -10),
        } },
        { "活発", new[] {
            new AxisDelta(PersonalityAxis.Shy,     -10),
            new AxisDelta(PersonalityAxis.Whimsy,   +5),
            new AxisDelta(PersonalityAxis.Mystery,  +5),
        } },
        { "おおらか", new[] {
            new AxisDelta(PersonalityAxis.Mystery, -10),
            new AxisDelta(PersonalityAxis.Caring,  +10),
        } },
        { "すなお", new[] {
            new AxisDelta(PersonalityAxis.Shy,   -10),
            new AxisDelta(PersonalityAxis.Spoil, +10),
        } },
        { "真面目", new[] {
            new AxisDelta(PersonalityAxis.Spoil,  -5),
            new AxisDelta(PersonalityAxis.Caring, +5),
        } },
        { "じっくり", new[] {
            new AxisDelta(PersonalityAxis.Lonely, +10),
            new AxisDelta(PersonalityAxis.Spoil,  -10),
        } },
        { "クール", new[] {
            new AxisDelta(PersonalityAxis.Spoil,  -5),
            new AxisDelta(PersonalityAxis.Shy,    -5),
            new AxisDelta(PersonalityAxis.Whimsy, -8),
            new AxisDelta(PersonalityAxis.Caring, +2),
        } },
        { "しっかり", new[] {
            new AxisDelta(PersonalityAxis.Whimsy, -8),
            new AxisDelta(PersonalityAxis.Caring, +8),
        } },
    };

    public static bool IsKnown(string keyword) {
        return !string.IsNullOrEmpty(keyword) && _table.ContainsKey(keyword.Trim());
    }

    /// <summary>選ばれたキーワードから性格を組み立てる(全軸50スタート、0-100にクランプ)</summary>
    public static PersonalitySnapshot Build(IEnumerable<string> keywords) {
        var result = new PersonalitySnapshot();

        foreach (PersonalityAxis axis in Enum.GetValues(typeof(PersonalityAxis)))
            result.Set(axis, BaseValue);

        if (keywords == null) return result;

        foreach (var keyword in keywords) {
            if (string.IsNullOrEmpty(keyword)) continue;

            AxisDelta[] deltas;
            if (!_table.TryGetValue(keyword.Trim(), out deltas)) continue;

            foreach (var delta in deltas) result.Add(delta.axis, delta.value);
        }

        return result;
    }
}
