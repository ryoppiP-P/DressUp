using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "DressUp/ItemDatabase")]
public class ItemDatabase : ScriptableObject {
    public List<DressUpItem> allItems;
    public DressUpItem Find(string itemName) => allItems.Find(i => i.itemName == itemName);
}
