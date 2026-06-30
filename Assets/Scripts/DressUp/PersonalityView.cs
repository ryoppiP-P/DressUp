using UnityEngine;
using TMPro;

public class PersonalityView : MonoBehaviour {
    [SerializeField] private CharacterManager charaManager;
    [SerializeField] private PersonalityLabelTable labelTable;
    [SerializeField] private RadarChart radarChart;

    [System.Serializable]
    public class AxisUI {
        public PersonalityAxis axis;
        public TMP_Text labelText;
    }
    [SerializeField] private AxisUI[] axisUIs; // 6ŒÂ

    void Start() { Refresh(); }

    public void Refresh() {
        int[] values = new int[6];

        foreach (var ui in axisUIs) {
            int value = charaManager.GetData(ui.axis);
            values[(int)ui.axis] = value;
            if (ui.labelText != null)
                ui.labelText.text = labelTable.GetLabel(ui.axis, value);
        }

        if (radarChart != null) radarChart.SetValues(values);
    }
}
