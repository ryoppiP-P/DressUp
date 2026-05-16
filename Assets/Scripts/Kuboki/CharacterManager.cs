/*
* ファイル名 CharacterManager.cs
* タイトル   キャラクターマネージャー
* 作成者   　久保木幹太
* 作成日   　5月15日
* 更新日   　5月15日
*/

using System;
using System.Collections.Generic;
using UnityEngine;

// キャラクターの構成要素
[Serializable]
public struct CharaData
{
    public string Name;   // 特徴の名前
    [Range(0, 100)] public int Parameter; // 特徴のパラメーター
}

public class CharacterManager : MonoBehaviour
{
    [Header("特徴データ")]
    public List<CharaData> dataList = new List<CharaData>();

    // ゲッター
    public int GetData(string name)
    {
        foreach (var pair in dataList)
        {
            if (pair.Name == name) return pair.Parameter;
        }
        Debug.LogWarning($"{name} というステータスは見つかりませんでした");

        return 0;
    }

    // セッター
    public void SetData(string name, int newValue)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i].Name == name)
            {
                // 構造体の値を更新してリストに戻す
                CharaData updatedPair = dataList[i];
                updatedPair.Parameter = newValue;
                dataList[i] = updatedPair;
                return;
            }
        }
        Debug.LogWarning($"{name} が見つからないため更新できませんでした");
    }

    public void AddData(string name, int addValue)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i].Name == name)
            {
                CharaData updatedPair = dataList[i];
                updatedPair.Parameter += addValue;
                dataList[i] = updatedPair;
                return;
            }
        }
        Debug.LogWarning($"{name} が見つからないため更新できませんでした");
    }
}