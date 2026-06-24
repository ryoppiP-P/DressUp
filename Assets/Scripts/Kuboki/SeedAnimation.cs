/*
* ファイル名　SeedAnimation.cs
* タイトル　　種動く
* 作成者　　　久保木幹太
* 作成日　　　6月22日
* 更新日　　　6月22日
*/

using UnityEngine;
using UnityEngine.UI;

public class SeedAnimation : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject prefabObjects;
    [SerializeField] private int effectCount;
    private bool isMove = false;

    void Start()
    {
        if (targetGameObject == null) targetGameObject = GetComponent<GameObject>();
    }

    private void Update()
    {
        if (button == null) return;
        // ボタンが押されたときの処理
        button.onClick.AddListener(() =>
        {
            if (targetGameObject != null)
            {
                if(!isMove) isMove = true;
            }
        });

        if (targetGameObject.transform.position.y < this.transform.position.y &&
            isMove)
        {
            this.transform.position += new Vector3(0.0f, -1.0f * moveSpeed, 0.0f) * Time.deltaTime;
        }

        if (targetGameObject.transform.position.y > this.transform.position.y)
        {
            Destroy(this.gameObject);
            int count = 0;
            while (count < effectCount)
            {
                GameObject newObj = Instantiate(prefabObjects, this.transform.position, Quaternion.identity);
                count++;
            }
        }
    }
}