using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterNaming : MonoBehaviour {
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text resultText;

    private Character character => DressUpTarget.Instance != null
        ? DressUpTarget.Instance.Current : null;

    void Start() {
        if (character != null)
            nameInput.text = character.DisplayName;

        confirmButton.onClick.AddListener(Confirm);
    }

    void Confirm() {
        if (character == null) {
            Debug.Log("[Naming] 対象キャラがいません");
            return;
        }

        string name = nameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) {
            Debug.Log("[Naming] 名前が空です");
            return;
        }

        character.SetDisplayName(name);
        Debug.Log($"[Naming] 名前を「{name}」に設定");

        if (resultText != null)
            resultText.text = $"{name} になったよ～";
    }
}
