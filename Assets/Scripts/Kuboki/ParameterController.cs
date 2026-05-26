/*
* ファイル名 ParameterController.cs
* タイトル   パラメーター操作
* 作成者   　久保木幹太
* 作成日   　5月15日
* 更新日   　5月15日
*/

using UnityEngine;
using UnityEngine.InputSystem;

public class ParameterController : MonoBehaviour
{
    public CharacterManager CM;

    [SerializeField] private string Name;
    [SerializeField] private int Value;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CM.AddData(Name, Value);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CM.AddData(Name, -Value);
        }
    }
}