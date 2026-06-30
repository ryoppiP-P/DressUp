using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "DressUp/PersonalityLabels")]
public class PersonalityLabelTable : ScriptableObject {
    [System.Serializable]
    public class LabelRange {
        public int min;       // この範囲の下限（含む）
        public int max;       // この範囲の上限（含む）
        public string label;
    }

    [System.Serializable]
    public class AxisLabels {
        public PersonalityAxis axis;
        public List<LabelRange> ranges; // 値の範囲ごとのラベル
    }

    public List<AxisLabels> table;

    public string GetLabel(PersonalityAxis axis, int value) {
        var a = table.Find(x => x.axis == axis);
        if (a == null) return "";
        foreach (var r in a.ranges)
            if (value >= r.min && value <= r.max) return r.label;
        return "";
    }
}