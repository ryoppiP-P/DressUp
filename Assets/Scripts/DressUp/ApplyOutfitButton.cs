using UnityEngine;
using UnityEngine.UI;

public class ApplyOutfitButton : MonoBehaviour {
    [SerializeField] private Button applyButton;

    private Character character => DressUpTarget.Instance != null
        ? DressUpTarget.Instance.Current : null;

    void Start() {
        applyButton.onClick.AddListener(OnApply);
    }

    void OnApply() {
        if (character != null) character.ApplyOutfit();
    }
}
