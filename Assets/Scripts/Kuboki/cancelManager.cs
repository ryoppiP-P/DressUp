using UnityEngine;
using UnityEngine.UI;

public class cancelManager : MonoBehaviour
{
    [SerializeField] private GameObject[] toggleObject;
    [SerializeField] private GameObject seedText;
    [SerializeField] private Toggle toggle;
    [SerializeField] private ToggleSlotManager slotManager;

    private void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            foreach (var obj in toggleObject)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            if (seedText != null)
            {
                seedText.SetActive(true);
            }

            if (slotManager != null)
            {
                slotManager.ResetSelectedToggles();
            }

            toggle.isOn = false; // ƒgƒOƒ‹‚ðOFF‚É‚·‚é
        }
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}