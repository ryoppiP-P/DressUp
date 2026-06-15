using System.Collections.Generic;

public class SavedOutfit {
    public Dictionary<CategoryType, DressUpItem> items = new();

    public void Capture(EquipState state) {
        items.Clear();
        foreach (var pair in state.equipped)
            items[pair.Key] = pair.Value;
    }
}
