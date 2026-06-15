using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour {
    [SerializeField] private Character character;
    [SerializeField] private Button resetButton;

    void Start() {
        resetButton.onClick.AddListener(OnReset);
    }

    void OnReset() {
        character.UnequipAll();
    }
}
