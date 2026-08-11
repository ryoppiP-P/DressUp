//==============================================================================
//  File   : TalkDatabase.cs
//  Brief  : TalkData一覧を保持し、話題・性格に応じてランダムに1本選ぶ
//==============================================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Talk/TalkDatabase")]
public class TalkDatabase : ScriptableObject {
    public List<TalkData> entries = new List<TalkData>();

    // ランダムに話題を決め、その話題+性格が一致するものを優先して1本選ぶ。
    // 一致するものが無ければ話題一致だけで選び、話題も無ければ全体から選ぶ。
    // includePlaceTopic が false の場合、話題として Place を選ばない
    // (向かっている建物が無く {place} を解決できない場合に使う)。
    public TalkData PickRandom(PersonalityAxis personality, bool includePlaceTopic) {
        if (entries == null || entries.Count == 0) return null;

        var valid = entries.Where(e => e != null && e.lines != null && e.lines.Length > 0).ToList();
        if (valid.Count == 0) return null;

        TalkTopic topic = RandomTopic(includePlaceTopic);

        var byTopic = valid.Where(e => e.topic == topic).ToList();
        if (byTopic.Count == 0) byTopic = valid;

        var byPersonality = byTopic.Where(e => e.personality == personality).ToList();
        var pool = byPersonality.Count > 0 ? byPersonality : byTopic;

        return pool[Random.Range(0, pool.Count)];
    }

    private TalkTopic RandomTopic(bool includePlaceTopic) {
        var values = System.Enum.GetValues(typeof(TalkTopic)).Cast<TalkTopic>()
            .Where(t => includePlaceTopic || t != TalkTopic.Place)
            .ToList();
        return values[Random.Range(0, values.Count)];
    }
}
