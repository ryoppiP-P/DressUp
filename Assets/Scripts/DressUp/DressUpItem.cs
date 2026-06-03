using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "DressUp/Item")]
public class DressUpItem : ScriptableObject {
    public string itemName;
    public CategoryType category;
    public Sprite icon;           // グリッドに表示するアイコン
    public Sprite previewSprite;  // キャラのレイヤーに差し込むスプライト

    [System.Serializable]
    public class StateAnim {
        public CharaState state;
        public Sprite[] frames;
    }

    public List<StateAnim> animations; // 状態ごとのコマ
    public float frameRate = 8f;

    public Sprite[] GetFrames(CharaState state) {
        var a = animations.Find(x => x.state == state);
        return (a != null) ? a.frames : null;
    }
}
