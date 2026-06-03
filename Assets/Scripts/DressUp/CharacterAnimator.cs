using UnityEngine;
using System.Collections.Generic;

public class CharacterAnimator : MonoBehaviour {
    [System.Serializable]
    public class Layer {
        public CategoryType category;
        public SpriteRenderer renderer;
        [HideInInspector] public DressUpItem item;
    }

    [SerializeField] private List<Layer> layers;
    [SerializeField] private float frameRate = 8f;

    private CharaState _state = CharaState.Idle;
    private float _timer;
    private int _frame;

    void Start() {
        if (PlayerEquipManager.Instance == null) return;

        var state = PlayerEquipManager.Instance.State;
        foreach (var pair in state.equipped) {
            var layer = layers.Find(l => l.category == pair.Key);
            if (layer != null) layer.item = pair.Value;
        }
    }

    // 状態を切り替える窓口。これを外から呼ぶだけ
    public void SetState(CharaState newState) {
        if (_state == newState) return;
        _state = newState;
        _frame = 0;    // 状態が変わったらフレームを頭から
        _timer = 0f;
    }

    void Update() {
        _timer += Time.deltaTime;
        float interval = 1f / Mathf.Max(1f, frameRate);
        if (_timer < interval) return;
        _timer -= interval;
        _frame++;

        foreach (var layer in layers) {
            if (layer.item == null) continue;
            var frames = layer.item.GetFrames(_state);
            if (frames != null && frames.Length > 0)
                layer.renderer.sprite = frames[_frame % frames.Length];
            else
                layer.renderer.sprite = layer.item.previewSprite;
        }
    }
}
