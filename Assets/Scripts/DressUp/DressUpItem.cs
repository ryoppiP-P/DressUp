// 入手順は、セーブデータ作り次第導入
using UnityEngine;
using System.Collections.Generic;

public enum ItemColor {
    Red,         // レッド
    Orange,      // オレンジ
    Yellow,      // イエロー
    YellowGreen, // 黄緑
    Green,       // グリーン
    Blue,        // ブルー
    Purple,      // パープル
    Pink,        // ピンク
    Brown,       // ブラウン
    Beige,       // ベージュ
    Gray,        // グレー
    White,       // ホワイト
    Black,       // ブラック
    Gold,        // ゴールド
    Silver,      // シルバー
    Colorful,    // カラフル
    Clear,       // クリア
}

[CreateAssetMenu(menuName = "DressUp/Item")]
public class DressUpItem : GameItem {
    public CategoryType category; // 着用カテゴリ(ヘア/トップ等)
    public Sprite previewSprite;  // キャラのレイヤーに差し込むスプライト

    // Set items (e.g. one-piece + choker). Equipping or removing this item also
    // equips/removes every item listed here.
    public List<DressUpItem> setItems = new List<DressUpItem>();
    // True for an item that only exists as part of another item's set.
    // It is hidden from the dress-up grid and the item list.
    public bool isSetPart = false;

    public int releaseYear = 2026;          // リリース年（絞り込み・並べ替え用）
    public ItemColor[] colors;       // このアイテムが持つ色（複数可）

    [System.Serializable]
    public class StateAnim {
        public CharaState state;
        public Sprite[] frames;

        // Cropped frames (Study etc. exported per part at different sizes):
        // place the sprite by offset/scale instead of fitting a 500px canvas.
        public bool usePlacement;
        public Vector2 offset;      // sprite center in 500px-canvas pixels (origin = canvas center, y up)
        public float scale = 1f;    // 1 = one sprite pixel is one canvas pixel
        public List<ExtraPart> extras = new List<ExtraPart>(); // arms, right shoe, eyebrows...
    }

    // An extra sprite drawn together with this item in a state (e.g. sleeve over the book)
    [System.Serializable]
    public class ExtraPart {
        public string name;
        public Sprite[] frames;
        public Vector2 offset;
        public float scale = 1f;
        public int sortingOrder = 30;
    }

    public StateAnim GetAnim(CharaState state) {
        if (animations == null) return null;
        return animations.Find(x => x.state == state && x.frames != null && x.frames.Length > 0);
    }

    public List<StateAnim> animations; // 状態ごとのコマ

    public Sprite[] GetFrames(CharaState state) {
        var a = animations.Find(x => x.state == state);
        return (a != null) ? a.frames : null;
    }
}