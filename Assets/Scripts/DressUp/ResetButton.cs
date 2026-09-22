using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour {
    [SerializeField] private Character character;
    [SerializeField] private Button resetButton;

    void Start() {
        resetButton.onClick.AddListener(OnReset);
    }

    void OnReset() {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySE(SEType.Tap);
        character.UnequipAll();
    }
}
