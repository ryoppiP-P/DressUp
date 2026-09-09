using UnityEngine;
using UnityEngine.UI;

public class ApplyOutfitButton : MonoBehaviour {
    [SerializeField] private Button applyButton;

    [Header("適用したときに出す「適用OK！」トースト(任意)")]
    [SerializeField] private SimpleMessagePopup appliedPopup;
    [SerializeField] private string appliedMessage = "適用OK!";

    private Character character => DressUpTarget.Instance != null
        ? DressUpTarget.Instance.Current : null;

    void Start() {
        applyButton.onClick.AddListener(OnApply);
    }

    void OnApply() {
        if (character == null) return;

        character.ApplyOutfit();
        if (appliedPopup != null) appliedPopup.Show(appliedMessage);
    }
}
