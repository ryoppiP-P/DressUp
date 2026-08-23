//==============================================================================
//  File   : FairyPersonalityMessage.cs
//  Brief  : 育てている種の性格から「〇〇な子が生まれそうだ！」を決める
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/23
//------------------------------------------------------------------------------
//==============================================================================
using UnityEngine;

public static class FairyPersonalityMessage {
    /// <summary>設計書 III-I の文章一覧(No順 = PersonalityAxis の順)</summary>
    private static readonly string[] Messages = {
        "ふしぎな子が生まれそうだ！",         // 1. ふしぎ
        "さみしがりなコが生まれそうだ！",     // 2. さみしがり
        "てれてれな子が生まれそうだ！",       // 3. てれや
        "優しい子が生まれそうだ！",           // 4. めんどうみ
        "きまぐれな子が生まれそうだ！",       // 5. きまぐれ
        "あまえが多い子が生まれそうだ！",     // 6. あまえ
    };

    /// <summary>性格から文章を1つ選ぶ。渡されなければ空文字。</summary>
    public static string For(PersonalitySnapshot personality) {
        if (personality == null) return "";

        int best = 0;
        int bestValue = personality.Get((PersonalityAxis)0);

        for (int i = 1; i < Messages.Length; i++) {
            int value = personality.Get((PersonalityAxis)i);

            // 同じ値の時は更新しない = No が小さい方(一覧で上)が勝つ
            if (value <= bestValue) continue;

            bestValue = value;
            best = i;
        }

        return Messages[best];
    }

    /// <summary>その鉢で育てている種の文章。植わっていなければ空文字。</summary>
    public static string ForSlot(int slotIndex) {
        var slot = FairySaveBridge.GetSlot(slotIndex);
        if (slot == null || !slot.isPlanted) return "";

        return For(slot.personality);
    }
}
