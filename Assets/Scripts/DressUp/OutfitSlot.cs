using UnityEngine;
using UnityEngine.UI;
using System;

public class OutfitSlot : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Image frameImage; // èÌÇ…ï\é¶ÇÃòg
    [SerializeField] private Image thumbImage; // ï€ë∂é ê^ÇèÊÇπÇÈ

    private SavedOutfit _outfit;
    private Action<SavedOutfit> _onLoad;
    private Action _onDelete;

    void Awake() {
        button.onClick.AddListener(OnClicked);
        if (deleteButton) deleteButton.onClick.AddListener(OnDelete);
        Clear();
    }

    public void Show(SavedOutfit outfit, Sprite thumbnail, Action<SavedOutfit> onLoad, Action onDelete) {
        _outfit = outfit;
        _onLoad = onLoad;
        _onDelete = onDelete;
        thumbImage.sprite = thumbnail;
        thumbImage.enabled = true; // ògÇÃè„Ç…é ê^ÇèÊÇπÇÈ
        if (deleteButton) deleteButton.gameObject.SetActive(true); // ï€ë∂çœÇ›Ç»ÇÁÅ~ï\é¶
    }

    public void Clear() {
        _outfit = null;
        _onDelete = null;
        thumbImage.enabled = false; // ògÇæÇØå©Ç¶ÇÈèÛë‘
        if (deleteButton) deleteButton.gameObject.SetActive(false); // ãÛÇ»ÇÁÅ~îÒï\é¶
    }

    void OnClicked() {
        if (_outfit != null) _onLoad?.Invoke(_outfit);
    }

    void OnDelete() {
        if (_outfit != null) _onDelete?.Invoke();
    }

    public bool IsEmpty => _outfit == null;
}
