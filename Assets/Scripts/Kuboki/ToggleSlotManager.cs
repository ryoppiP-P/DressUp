/*
* ファイル名　ToggleSlotManager.cs
* タイトル　　トグルのスロット管理
* 作成者　　　久保木幹太
* 作成日　　　6月22日
* 更新日　　　6月22日
*/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ToggleSlotManager : MonoBehaviour
{
    public Transform[] slots;      // 移動先ポジション（3個）
    public GameObject ToggleUI;    // Toggleパネル
    public GameObject confirmUI;   // YES/NOパネル
    public Button noButton;        // NOボタン

    // 現在選択中のトグルリスト
    private List<ToggleMove> selectedToggles = new List<ToggleMove>();

    void Start()
    {
        confirmUI.SetActive(false);
        noButton.onClick.AddListener(HideConfirmUI);
    }

    // トグルがONになった時
    public void AddToggle(ToggleMove toggle)
    {
        if (selectedToggles.Count < 3)
        {
            selectedToggles.Add(toggle);
            UpdateLayout();
        }
        else
        {
            // 3個以上の場合はONにさせない
            toggle.GetComponent<Toggle>().isOn = false;
        }
    }

    // トグルがOFFになった時
    public void RemoveToggle(ToggleMove toggle)
    {
        if (selectedToggles.Contains(toggle))
        {
            selectedToggles.Remove(toggle);
            toggle.ReturnToOriginal();
            UpdateLayout();
        }
    }

    // リストの順番通りにポジションを再割り当てする（左詰めロジック）
    void UpdateLayout()
    {
        for (int i = 0; i < selectedToggles.Count; i++)
        {
            selectedToggles[i].MoveTo(slots[i].position);
        }

        // 3つ揃ったらUI表示
        if (selectedToggles.Count == 3)
        {
            confirmUI.SetActive(true);
            ToggleUI.SetActive(false); // Toggleパネルを非表示にする
        }
        else
        {
            confirmUI.SetActive(false);
            ToggleUI.SetActive(true); // Toggleパネルを表示
        }
    }

    void HideConfirmUI()
    {
        confirmUI.SetActive(false);
        ToggleUI.SetActive(true); // Toggleパネルを表示
    }
}