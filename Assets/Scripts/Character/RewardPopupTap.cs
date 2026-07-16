using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RewardPopupTap : MonoBehaviour {
    [SerializeField] private CharacterReward owner;

    void OnMouseDown() {
        if (owner != null) owner.OnPopupTapped();
    }
}
