using UnityEngine;
using TMPro;

public class ItemSearch : MonoBehaviour {
    [SerializeField] private DressupGrid grid;
    [SerializeField] private TMP_InputField input;

    void Start() {
        input.onValueChanged.AddListener(OnSearch);
    }

    void OnSearch(string keyword) {
        grid.ShowByName(keyword);
    }
}
