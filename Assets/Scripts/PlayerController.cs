using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{  public Transform viewPoint;//カメラの位置オブジェクト
    public float mouseSensitivity = 1f;//視点移動の速度
    private Vector2 mouseInput;//ユーザーのマウス入力を格納
    private float verticalMouseInput;//y軸の回転を格納　回転を制限したいから
    private Camera cam;//カメラ



    private Vector3 moveDir;//プレイヤーの入力を格納（移動）
    private Vector3 movement;//進む方向を格納する変数
    private float activeMoveSpeed = 4;//実際の移動速度



    public Vector3 jumpForce = new Vector3(0, 6, 0);//ジャンプ力 
    public Transform groundCheckPoint;//地面に向けてレイを飛ばすオブジェクト 
    public LayerMask groundLayers;//地面だと認識するレイヤー 
    Rigidbody rb;//


    public float walkSpeed = 4f, runSpeed = 8f;//歩きの速度、走りの速度


    private bool cursorLock = true;//カーソルの表示/非表示 


    public List<Gun> guns = new List<Gun>();//武器の格納配列
    private int selectedGun = 0;//選択中の武器管理用数値


    private float shotTimer;//射撃間隔
    [Tooltip("所持弾薬")]
    public int[] ammunition;
    [Tooltip("最高所持弾薬数")]
    public int[] maxAmmunition;
    [Tooltip("マガジン内の弾数")]
    public int[] ammoClip;
    [Tooltip("マガジンに入る最大の数")]
    public int[] maxAmmoClip;

    
    UIManager uIManager;//UI管理


    SpawnManager spawnManager;//スポーンマネージャー管理


    private void Awake()
    {
        //タグからUIManagerを探す
        uIManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        //タグからSpawnManagerを探す
        spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
    }

    private void Start()
    {
        //変数にメインカメラを格納
        cam = Camera.main;


        //Rigidbodyを格納
        rb = GetComponent<Rigidbody>();


        //カーソル非表示
        UpdateCursorLock();


        //弾薬テキスト更新
        uIManager.SettingBulletsText(ammoClip[selectedGun], ammunition[selectedGun]);


        //ランダムな位置でスポーンさせる
        transform.position = spawnManager.GetSpawnPoint().position;
    }

    private void Update()
    {
        //視点移動関数
        PlayerRotate();

        //移動関数
        PlayerMove();

        //地面についているのか判定をする
        if (IsGround())
        {
            //走りの関数を呼ぶ
            Run();

            //ジャンプ関数を呼ぶ
            Jump();
        }

        //銃の切り替え
        SwitchingGuns();

        //覗き込み
        Aim();

        //射撃関数
        Fire();

        //リロード関数
        Reload();

        //カーソル非表示
        UpdateCursorLock();
    }

    //Update関数が呼ばれた後に実行される
    private void LateUpdate()
    {
        //カメラをプレイヤーの子にするのではなく、スクリプトで位置を合わせる
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    //初期設定では0.02秒ごとに呼ばれる
    private void FixedUpdate()
    {
        //弾薬テキスト更新
        uIManager.SettingBulletsText(ammoClip[selectedGun], ammunition[selectedGun]);
    }

    /// <summary>
    /// Playerの横回転と縦の視点移動を行う
    /// </summary>
    public void PlayerRotate()
    {
        //変数にユーザーのマウスの動きを格納
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity,
            Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

        //横回転を反映(transform.eulerAnglesはオイラー角としての角度が返される)
        transform.rotation = Quaternion.Euler
            (transform.eulerAngles.x, 
            transform.eulerAngles.y + mouseInput.x, //マウスのx軸の入力を足す
            transform.eulerAngles.z);


        //変数にy軸のマウス入力分の数値を足す
        verticalMouseInput += mouseInput.y;

        //変数の数値を丸める（上下の視点制御）
        verticalMouseInput = Mathf.Clamp(verticalMouseInput, -60f, 60f);

        //縦の視点回転を反映(-を付けないと上下反転してしまう)
        viewPoint.rotation = Quaternion.Euler
            (-verticalMouseInput, 
            viewPoint.transform.rotation.eulerAngles.y, 
            viewPoint.transform.rotation.eulerAngles.z);
    }

    

    /// <summary>
    /// Playerの移動
    /// </summary>
    public void PlayerMove()
    {
        //変数の水平と垂直の入力を格納する（wasdや矢印の入力）
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 
            0, Input.GetAxisRaw("Vertical"));

        //Debug.Log(moveDir);説明用

        //ゲームオブジェクトのｚ軸とx軸に入力された値をかけると進む方向が出せる
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized;

        //現在位置に進む方向＊移動スピード＊フレーム間秒数を足す
        transform.position += movement * activeMoveSpeed * Time.deltaTime;
    }


    /// <summary>
    /// 地面についているならtrue
    /// </summary>
    /// <returns></returns>
    public bool IsGround()
    {
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers);
    }

    
    public void Jump()
    {
        //ジャンプできるのか判定
        if (IsGround() && Input.GetKeyDown(KeyCode.Space))
        {
            //瞬間的に真上に力を加える
            rb.AddForce(jumpForce, ForceMode.Impulse);
        }
    }


    public void Run()
    {
        //左シフトを押しているときはスピードを切り替える
        if(Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = walkSpeed;
        }
    }


    public void UpdateCursorLock()
    {
        //入力しだいでcursorLockを切り替える
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLock = false;
        }
        else if (Input.GetMouseButton(0))
        {
            cursorLock = true;
        }

        //cursorLock次第でカーソルの表示を切り替える
        if (cursorLock)
        {
            //カーソルを中央に固定し、非表示　https://docs.unity3d.com/ScriptReference/CursorLockMode.html
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            //表示
            Cursor.lockState = CursorLockMode.None;
        }
    }


    /// <summary>
    /// 銃の切り替えキー入力を検知する
    /// </summary>
    public void SwitchingGuns()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun++;//扱う銃を管理する数値を増やす

            //リストより大きい数値になっていないか確認
            if (selectedGun >= guns.Count)
            {
                selectedGun = 0;//リストより大きな数値になれば０に戻す
            }

            //実際に武器を切り替える関数
            switchGun();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;//扱う銃を管理する数値を減らす


            if (selectedGun < 0)
            {
                selectedGun = guns.Count - 1;//0より小さければリストの最大数－１の数値に設定する
            }

            //実際に武器を切り替える関数
            switchGun();
        }

        //数値キーの入力検知で武器を切り替える
        for (int i = 0; i < guns.Count; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))//ループの数値＋１をして文字列に変換。その後、押されたか判定
            {
                selectedGun = i;//銃を扱う数値を設定

                //実際に武器を切り替える関数
                switchGun();

            }
        }
     
    }
    
    /// <summary>
    /// 銃の切り替え
    /// </summary>
    void switchGun()
    {
        foreach (Gun gun in guns)//リスト分ループを回す
        {
            gun.gameObject.SetActive(false);//銃を非表示
        }

        guns[selectedGun].gameObject.SetActive(true);//選択中の銃のみ表示
    }





    /// <summary>
    /// 右クリックで覗き込み
    /// </summary>
    public void Aim()
    {
        //  マウス右ボタン押しているとき
        if (Input.GetMouseButton(1))
        {
            //fieldOfViewコンポーネントの値を変更(開始地点、目的地点、補完数値)　　開始地点から目的地点まで補完数値の割合で徐々に近づける
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, guns[selectedGun].adsZoom, guns[selectedGun].adsSpeed * Time.deltaTime);
        }
        else
        {   //60は初期設定数値
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, guns[selectedGun].adsSpeed * Time.deltaTime);
        }
    }



    /// <summary>
    /// 左クリックの検知
    /// </summary>
    public void Fire()
    {
        
        if (Input.GetMouseButton(0) && ammoClip[selectedGun] > 0 && Time.time > shotTimer)
        {
            FiringBullet();
        }

    }

    /// <summary>
    /// 弾丸の発射
    /// </summary>
    private void FiringBullet()
    {
        //選択中の銃の弾薬減らす
        ammoClip[selectedGun]--;

        //Ray(光線)をカメラの中央からに設定
        Ray ray = cam.ViewportPointToRay(new Vector2(.5f, .5f));//カメラの中心がこの数値

        GameObject firepoint=guns[selectedGun].transform.GetChild(0).gameObject;
        GameObject Ammo=Instantiate(guns[selectedGun].Ammo,firepoint.transform.position,Quaternion.Euler(transform.eulerAngles.x,transform.eulerAngles.y,0));
        Rigidbody rigid=Ammo.GetComponent<Rigidbody>();
        rigid.AddForce(transform.forward*10);
        //レイを飛ばす（開始地点と方向、当たったコライダーの情報格納）
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log("当たったオブジェクトは" + hit.collider.gameObject.name);

            //弾痕エフェクト (hit.pointはコライダーにヒットした位置)：hit.point + (hit.normal * .002f)はちらつかないように少し上にしている
            //hit normalは当たったオブジェクトに対して直角の方向が返される
            //LookRotationは指定した方向に回す
            GameObject bulletImpactObject = Instantiate(guns[selectedGun].bulletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));

            //時間経過で消えるようにする
            Destroy(bulletImpactObject, 10f);
        }

        //射撃間隔を設定
        shotTimer = Time.time + guns[selectedGun].shootInterval;


    }


    /// <summary>
    /// リロード
    /// </summary>
    private void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //リロードで補充する弾数を取得する
            int amountNeed = maxAmmoClip[selectedGun] - ammoClip[selectedGun];

            //必要な弾薬量と所持弾薬量を比較
            int ammoAvailable = amountNeed < ammunition[selectedGun] ? amountNeed : ammunition[selectedGun];

            //弾薬が満タンの時はリロードできない&弾薬を所持しているとき
            if (amountNeed != 0 && ammunition[selectedGun] != 0)
            {
                //所持弾薬からリロードする弾薬分を引く
                ammunition[selectedGun] -= ammoAvailable;
                //銃に装填する
                ammoClip[selectedGun] += ammoAvailable;
            }
        }
    }
}
