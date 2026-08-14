//==============================================================================
//  File   : FairyBirthFlow.cs
//  Brief  : 誕生フロー(畑 → 着せ替え → 命名 → 街)のシーンをまたぐ受け渡し
//
//  Author : Ryoto Kikuchi
//  Date   : 2026/8/13
//------------------------------------------------------------------------------
//  CharacterSelection と同じ「static に持つだけ」の方針。
//  アプリを落とした場合の復帰は、セーブ側の namingDone = false を見て判断する
//  (FairySaveBridge.FindUnnamed)。
//==============================================================================

public static class FairyBirthFlow {
    /// <summary>着せ替え画面で名前を付ける対象。誕生フロー中でなければ空。</summary>
    public static string PendingNamingId;

    public static bool IsNamingFlow {
        get { return !string.IsNullOrEmpty(PendingNamingId); }
    }

    public static void Begin(string characterId) {
        PendingNamingId = characterId;
    }

    public static void Finish() {
        PendingNamingId = null;
    }
}
