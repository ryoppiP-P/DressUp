/*
* ファイル名　SeedAnimation.cs
* タイトル　　種動く
* 作成者　　　久保木幹太
* 作成日　　　6月22日
* 更新日　　　6月22日
*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SeedAnimation : MonoBehaviour
{
    [SerializeField] private Button hyojiButton;
    [SerializeField] private GameObject seedObject;
    [SerializeField] private GameObject seedButton;
    [SerializeField] private Button button;
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject prefabObjects;
    [SerializeField] private int effectCount;
    private bool isMove = false;
    private Vector3 startPosition;

    void Start()
    {
        seedObject.gameObject.SetActive(false); // 最初は非表示

        startPosition = seedObject.transform.position; // 最初にポジションを保存
        if (targetGameObject == null) targetGameObject = GetComponent<GameObject>();
        if (seedObject == null) seedObject = GetComponent<GameObject>();
        if (seedButton == null) seedButton = GetComponent<GameObject>();

        // リスナーは一度だけ登録する
        if (hyojiButton != null)
        {
            hyojiButton.onClick.AddListener(() =>
            {
                // 表示する
                seedObject.gameObject.SetActive(true);
            });
        }

        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                if (targetGameObject != null)
                {
                    isMove = true;
                }
            });
        }
    }

    private void Update()
    {
        if (!isMove) return;

        if (targetGameObject.transform.position.y < seedObject.transform.position.y)
        {
            seedObject.transform.position += new Vector3(0.0f, -1.0f * moveSpeed, 0.0f) * Time.deltaTime;
        }
        else
        {
            isMove = false;
            // 非表示する
            seedObject.gameObject.SetActive(false);
            int count = 0;
            while (count < effectCount)
            {
                GameObject newObj = Instantiate(prefabObjects, seedObject.transform.position, Quaternion.identity);
                count++;
            }
            seedObject.gameObject.transform.position = startPosition; // 元の位置に戻す
            StartCoroutine(ButtonReset(2.0f));
        }
    }

    // コルーチン関数
    private IEnumerator ButtonReset(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        seedButton.SetActive(true); // ボタンを表示
    }
}