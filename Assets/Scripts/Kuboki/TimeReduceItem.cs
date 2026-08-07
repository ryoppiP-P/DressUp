using UnityEngine;

[CreateAssetMenu(fileName = "TimeReduceItem_", menuName = "Items/TimeReduceItem")]
public class TimeReduceItem : OtherItem
{
    [Header("ŠÔ’Zkİ’è")]
    [SerializeField] public float reduceSeconds = 600.0f;

    private void OnValidate()
    {
        itemType = OtherItemType.TimeReduce;
    }

    public override bool Use(GameObject target)
    {
        if (target == null) return false;

        // ‘ÎÛ‚©‚ç SeedTime ƒRƒ“ƒ|[ƒlƒ“ƒg‚ğæ“¾
        SeedTime targetSeed = target.GetComponent<SeedTime>();

        if (targetSeed != null && targetSeed.IsPlanted)
        {
            // ŠÔ‚ğ’Zk
            targetSeed.ReduceTime(reduceSeconds);
            Debug.Log($"{itemName} ‚ğg—p‚µ‚Ä {reduceSeconds} •b’Zk‚µ‚Ü‚µ‚½I");
            return true; // ¬Œ÷
        }

        Debug.LogWarning("‘ÎÛ‚ÉSeedTime ‚ª‚Â‚¢‚Ä‚¢‚È‚¢‚©í‚ªA‚¦‚ç‚ê‚Ä‚¢‚Ü‚¹‚ñB");
        return false; // ¸”s
    }
}