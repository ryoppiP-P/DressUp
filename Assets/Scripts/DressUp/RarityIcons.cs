using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DressUp/RarityIcons")]
public class RarityIconTable : ScriptableObject {
    [System.Serializable]
    public class Entry {
        public Rarity rarity;
        public Sprite icon;
    }

    public List<Entry> entries;

    public Sprite GetIcon(Rarity rarity) {
        var e = entries.Find(x => x.rarity == rarity);
        return (e != null) ? e.icon : null;
    }
}