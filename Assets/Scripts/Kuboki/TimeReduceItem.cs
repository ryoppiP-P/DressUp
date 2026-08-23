using UnityEngine;

[CreateAssetMenu(fileName = "TimeReduceItem_", menuName = "Items/TimeReduceItem")]
public class TimeReduceItem : OtherItem
{
    [Header("時間短縮設定")]
    [SerializeField] public float reduceSeconds = 600.0f;

    private void OnValidate()
    {
        itemType = OtherItemType.TimeReduce;
    }

    public override bool Use(GameObject target)
    {
        if (target == null) return false;

        // 対象から SeedTime コンポーネントを取得
        SeedTime targetSeed = target.GetComponent<SeedTime>();

        if (targetSeed != null && targetSeed.IsPlanted)
        {
            // 持っていなければ使えない(ショップで買った個数を1つ消費する)
            if (!ConsumableBridge.TryConsume(this, 1))
            {
                Debug.Log($"{itemName} を持っていません");
                return false;
            }

            // 時間を短縮
            targetSeed.ReduceTime(reduceSeconds);
            Debug.Log($"{itemName} を使用して {reduceSeconds} 秒短縮しました！(残り {ConsumableBridge.GetCount(this)}個)");
            return true; // 成功
        }

        Debug.LogWarning("対象にSeedTime がついていないか種が植えられていません。");
        return false; // 失敗
    }
}