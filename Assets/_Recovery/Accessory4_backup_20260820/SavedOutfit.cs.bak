using System.Collections.Generic;

public class SavedOutfit {
    public Dictionary<CategoryType, DressUpItem> items = new();

    public void Capture(Dictionary<CategoryType, DressUpItem> equipped) {
        items.Clear();
        foreach (var pair in equipped)
            items[pair.Key] = pair.Value;
    }
}
