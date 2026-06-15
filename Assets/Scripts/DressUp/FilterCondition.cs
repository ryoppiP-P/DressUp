using System.Collections.Generic;

public enum SortOption {
    AcquiredNew,   // 入手順 新しい
    AcquiredOld,   // 入手順 古い
    ReleaseNew,    // リリース順 新しい
    ReleaseOld,    // リリース順 古い
    RarityHigh,    // レアリティ順 高い
    RarityLow,     // レアリティ順 低い
}

public class FilterCondition {
    public SortOption sort = SortOption.AcquiredNew; // どれか1つ

    public string nameKeyword = "";
    public HashSet<Rarity> rarities = new();      // 空 = 全部許可
    public HashSet<ItemColor> colors = new();     // 空 = 全部許可
    public HashSet<int> releaseYears = new();     // 空 = 全部許可
}
