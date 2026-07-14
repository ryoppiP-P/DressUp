using UnityEngine;

public class DressUpTarget : MonoBehaviour {
    public static DressUpTarget Instance { get; private set; }

    public Character Current { get; private set; }
    public System.Action<Character> OnTargetChanged;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetTarget(Character character) {
        Current = character;
        OnTargetChanged?.Invoke(character);
    }
}