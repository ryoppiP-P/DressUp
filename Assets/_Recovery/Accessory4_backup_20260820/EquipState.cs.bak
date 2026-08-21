using System.Collections.Generic;

public class EquipState {
    public Dictionary<CategoryType, DressUpItem> equipped = new();

    public void Set(CategoryType category, DressUpItem item) {
        equipped[category] = item;
    }

    public void Clear(CategoryType category) {
        equipped.Remove(category);
    }

    public void ClearAll() {
        equipped.Clear();
    }
}
