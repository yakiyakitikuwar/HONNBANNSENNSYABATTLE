using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoType : MonoBehaviour
{
    [Tooltip("弾の速度")]
    public float AmmoSpeed;
    [Tooltip("弾の射程距離")]
    public float AmmoRange;
    [Tooltip("弾の反射回数")]
    public int AmmoReflectTime;
    [Tooltip("爆発可能かどうか")]
    public bool canexplosion;
    Rigidbody rigid;
    void Start()
    {
        rigid=GetComponent<Rigidbody>();
    }
    void Update()
    {
        rigid.AddForce(Vector3.up,ForceMode.Force);
    }
}
