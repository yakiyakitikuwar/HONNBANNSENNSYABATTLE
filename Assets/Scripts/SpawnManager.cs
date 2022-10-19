using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
   public Transform[] spawnPositons;


    private void Start()
    {
        //スポーンポイントオブジェクトをすべて非表示
        foreach (var pos in spawnPositons)
        {
            pos.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// リスポーン地点をランダム取得する関数
    /// </summary>
    public Transform GetSpawnPoint()
    {
        //ランダムでスポーンポイント１つ選んで位置情報を返す
        return spawnPositons[Random.Range(0, spawnPositons.Length)];
    }
}
