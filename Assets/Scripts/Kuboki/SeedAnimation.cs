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
    // エフェクトの置き場所。UIの上で出したいので Canvas の中を指定する。
    // 未指定なら今までどおりシーン直下に出る。
    [SerializeField] private Transform effectParent;
    [SerializeField] private int effectCount;
    [SerializeField] private GameObject[] hyoujiObject;
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

        // 芽は「その鉢に植わっているか」で出す。
        // 植わっている数だけ数えて先頭から出すと、
        // 鉢２や鉢３に植えても鉢１の芽が出てしまう。
        for (int i = 0; i < hyoujiObject.Length; i++)
            if (hyoujiObject[i] != null)
                hyoujiObject[i].SetActive(FairySaveBridge.IsPlanted(i));
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
                GameObject newObj = effectParent != null
                    ? Instantiate(prefabObjects, seedObject.transform.position, Quaternion.identity, effectParent)
                    : Instantiate(prefabObjects, seedObject.transform.position, Quaternion.identity);
                count++;
            }
            seedObject.gameObject.transform.position = startPosition; // 元の位置に戻す

            // 種を表示する
            int slot = FairyPotFocus.Current != null ? FairyPotFocus.Current.FocusedSlot : -1;

            if (slot >= 0 && slot < hyoujiObject.Length && hyoujiObject[slot] != null)
            {
                // 今開いている鉢に出す。
                // 種切れなどで植えられなかった時は芽を出さない。
                hyoujiObject[slot].SetActive(FairySaveBridge.IsPlanted(slot));
            }
            else
            {
                foreach (var obj in hyoujiObject)
                {
                    if (obj != null)
                    {
                        if (obj.activeSelf) continue; // すでに表示されている場合はスキップ

                        obj.SetActive(true);

                        break;
                    }
                }
            }

            StartCoroutine(ButtonReset(2.0f));
        }
    }

    // コルーチン関数
    private IEnumerator ButtonReset(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (seedButton != null) seedButton.SetActive(true); // ボタンを表示
    }
}