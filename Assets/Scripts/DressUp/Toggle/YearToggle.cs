using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class YearToggle : MonoBehaviour {
    public int year;
    public Toggle Toggle => GetComponent<Toggle>();
}