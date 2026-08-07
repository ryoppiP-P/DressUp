using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class seedManager : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private GameObject[] seacretObjects;
    [SerializeField] private GameObject[] hyoujiObjects;
    [SerializeField] private SeedTime seedTime;

    private void Start()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        foreach (var obj in hyoujiObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        if (seedTime != null)
        {
            seedTime.PlantSeed();
        }
    }

    private void Update()
    {
        if (toggle.isOn && seedTime != null)
        {
            seedTime.UpdateUI();
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            foreach (var obj in seacretObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            foreach (var obj in hyoujiObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
        else
        {
            foreach (var obj in seacretObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            foreach (var obj in hyoujiObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }
}