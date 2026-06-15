using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ColorToggle : MonoBehaviour {
    public ItemColor color;
    public Toggle Toggle => GetComponent<Toggle>();
}