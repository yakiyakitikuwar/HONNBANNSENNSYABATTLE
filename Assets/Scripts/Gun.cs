
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Tooltip("射撃間隔")]
    public float shootInterval = .1f;
    [Tooltip("威力")]
    public int shotDamage;
    [Tooltip("覗き込み時のズーム")]
    public float adsZoom;
    [Tooltip("覗き込み時の速度")]
    public float adsSpeed;
    [Tooltip("弾")]
    public GameObject Ammo;
    

    public GameObject bulletImpact;//弾痕オブジェクト
}
