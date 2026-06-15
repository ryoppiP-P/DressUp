using UnityEngine;
using UnityEngine.UI;
using System;

public class OutfitSlot : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image frameImage; // èÌÇ…ï\é¶ÇÃòg
    [SerializeField] private Image thumbImage; // ï€ë∂é ê^ÇèÊÇπÇÈ

    private SavedOutfit _outfit;
    private Action<SavedOutfit> _onLoad;

    void Awake() {
        button.onClick.AddListener(OnClicked);
        Clear();
    }

    public void Show(SavedOutfit outfit, Sprite thumbnail, Action<SavedOutfit> onLoad) {
        _outfit = outfit;
        _onLoad = onLoad;
        thumbImage.sprite = thumbnail;
        thumbImage.enabled = true; // ògÇÃè„Ç…é ê^ÇèÊÇπÇÈ
    }

    public void Clear() {
        _outfit = null;
        thumbImage.enabled = false; // ògÇæÇØå©Ç¶ÇÈèÛë‘
    }

    void OnClicked() {
        if (_outfit != null) _onLoad?.Invoke(_outfit);
    }

    public bool IsEmpty => _outfit == null;
}
