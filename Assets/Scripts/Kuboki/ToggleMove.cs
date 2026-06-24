/*
* ファイル名　ToggleMove.cs
* タイトル　　トグルの移動
* 作成者　　　久保木幹太
* 作成日　　　6月22日
* 更新日　　　6月22日
*/

using UnityEngine;
using UnityEngine.UI;

public class ToggleMove : MonoBehaviour
{
    [Header("引っ越し先のCanvasやPanelを指定")]
    public Transform targetCanvas;   // ここをインスペクターで指定できるようにする

    [HideInInspector] public Vector3 originalPosition; // 元の位置
    private Transform originalParent; // 元の親（ToggleGroupなど）

    private Toggle toggle;
    private ToggleSlotManager manager;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        originalPosition = transform.position; // 開始時の位置を記録
        originalParent = transform.parent;     // 開始時の親を記録

        manager = FindFirstObjectByType<ToggleSlotManager>(); // マネージャーを探す

        // トグルの値が変わった時のイベント
        toggle.onValueChanged.AddListener(OnToggleClicked);
    }

    void OnToggleClicked(bool isOn)
    {
        if (isOn)
        {
            manager.AddToggle(this);    // 選択リストへ追加
        }
        else
        {
            manager.RemoveToggle(this); // 選択リストから削除
        }
    }

    // 指定された位置へ移動する関数
    public void MoveTo(Vector3 targetPos)
    {
        // インスペクターで指定したCanvas（またはPanel）の子要素に引っ越しさせる
        if (targetCanvas != null)
        {
            transform.SetParent(targetCanvas);
        }
        else if (manager != null)
        {
            // もし設定を忘れていた場合のセーフティとしてマネージャーの親にする
            transform.SetParent(manager.transform.parent);
        }

        transform.position = targetPos;
    }

    public void ReturnToOriginal()
    {
        // キャンセルされたら、元の親（ToggleGroup）の部屋に戻す
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
        }

        transform.position = originalPosition;
    }
}