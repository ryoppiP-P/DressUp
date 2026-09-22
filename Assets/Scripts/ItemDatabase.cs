using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "DressUp/ItemDatabase")]
public class ItemDatabase : ScriptableObject {
    public List<DressUpItem> allItems;
    // itemName is not unique (season variants / HairFront+HairBack share names). Prefer FindById.
    public DressUpItem Find(string itemName) => allItems.Find(i => i.itemName == itemName);

    public DressUpItem FindById(string itemId) => allItems.Find(i => i != null && i.itemId == itemId);

    public DressUpItem FindByCategoryAndName(CategoryType category, string itemName)
        => allItems.Find(i => i != null && i.category == category && i.itemName == itemName);
}
