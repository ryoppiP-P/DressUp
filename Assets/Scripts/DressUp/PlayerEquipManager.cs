using UnityEngine;

public class PlayerEquipManager : MonoBehaviour {
    public static PlayerEquipManager Instance { get; private set; }

    public EquipState State { get; private set; } = new EquipState();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
