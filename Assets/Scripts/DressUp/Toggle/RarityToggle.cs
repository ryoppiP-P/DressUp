using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class RarityToggle : MonoBehaviour {
    public Rarity rarity;
    public Toggle Toggle => GetComponent<Toggle>();
}