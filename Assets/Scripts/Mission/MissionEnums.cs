//==============================================================================
//  File   : MissionEnums.cs
//  Brief  : ミッションのカテゴリ・種類・状態を定義する列挙型
// 
//  Author : Ryoto Kikuchi
//  Date   : 2026/7/30
//------------------------------------------------------------------------------
//
//==============================================================================
// ミッションのカテゴリ（タブ）
public enum MissionCategory {
    Daily,
    Weekly,
    Challenge,
}

// ミッションの種類（何をするか＝進捗のカウント対象）
public enum MissionType {
    PlaceDecoration,   // 街に装飾を置こう
    CollectReward,     // キャラクターから報酬を取得しよう
    Login,             // ログインしよう
    Gacha,             // ガチャをしよう（未実装：口だけ）
    ClearAllDaily,     // デイリーを全部クリアしよう
    ClearAllWeekly,    // ウィークリーを全部クリアしよう
    PlayTime,          // 何時間プレイしよう（チャレンジ）
    BornFairy,         // 妖精を誕生させよう（チャレンジ）
    MakePersonality,   // 各性格のを作ろう（チャレンジ）
    CollectClothes,    // 服を集めよう（チャレンジ）
}

// ミッション1件の状態
public enum MissionState {
    InProgress,   // 挑戦中（未達成）→ ボタン「挑戦する」
    Claimable,    // 達成・未受取     → ボタン「受け取る」
    Claimed,      // 受取済み         → ボタン「クリア」
}