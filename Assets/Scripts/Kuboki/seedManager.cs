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

        // 植わっているかはセーブが持っているので、それに合わせて表示を決める
        bool planted = (seedTime != null && seedTime.IsPlanted);

        foreach (var obj in hyoujiObjects) {
            if (obj != null) {
                obj.SetActive(planted);
            }
        }

        foreach (var obj in seacretObjects) {
            if (obj != null) {
                obj.SetActive(!planted);
            }
        }

        if (planted) {
            if (toggle != null) toggle.isOn = true;  // 残り時間の更新はUpdate内でtoggle.isOn依存
            if (seedTime != null) seedTime.UpdateUI();
        }

        // 種を植えるのは「願いを込める」でキーワードが決まった時だけ。
        // ここで自動的に植えてしまうと、性格が決まっていない種になってしまう。
        Debug.Log($"[畑] SaveManager={SaveManager.Instance != null} / seedTime={seedTime != null} / slot={(seedTime != null ? seedTime.SlotIndex : -1)} / planted={(seedTime != null && seedTime.IsPlanted)} / 残り={(seedTime != null ? seedTime.GetRemainingTime() : 0f)}");
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