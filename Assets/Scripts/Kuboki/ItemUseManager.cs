using UnityEngine;
using UnityEngine.UI;

public class ItemUseManager : MonoBehaviour
{
    [Header("使用するボタン")]
    [SerializeField] private Button myButton;

    [Header("対象となる種のGameObject")]
    [SerializeField] private GameObject targetSeedObject;

    [Header("使用するアイテム（OtherItem型でセット可能）")]
    [SerializeField] private OtherItem itemToUse;

    private void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnClickUseItem);
        }
    }

    public void OnClickUseItem()
    {
        if (itemToUse != null && targetSeedObject != null)
        {
            bool success = itemToUse.Use(targetSeedObject);

            if (success)
            {
                Debug.Log("アイテムの消費処理");
            }
        }
    }
}