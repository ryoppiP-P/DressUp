using UnityEngine;
using System.Collections.Generic;

public class SortToggleGroup : MonoBehaviour {
    [SerializeField] private List<SortToggle> toggles;

    public SortOption Current {
        get {
            foreach (var t in toggles)
                if (t.Toggle.isOn) return t.option;
            return SortOption.AcquiredNew; // ‰½‚à‘I‚Î‚ê‚Ä‚¢‚È‚¢Žž‚ÌŠù’è
        }
    }
}
