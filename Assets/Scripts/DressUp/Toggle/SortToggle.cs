using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SortToggle : MonoBehaviour {
    public SortOption option;
    public Toggle Toggle => GetComponent<Toggle>();
}