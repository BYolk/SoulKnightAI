using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region

    #endregion






    #region 单例

    /// <summary>
    /// 单例
    /// </summary>
    public static Player instance;
    #endregion






    #region 玩家相关变量引用
    //变量：血量，魔法值，移动速度；玩家移动后的新坐标；玩家移动方向；玩家当前所看方向
    public int hpValue = 100;            
    public int magicValue = 100;
    int currentHPValue = 100;
    int currentMagicValue = 100;
    public float moveSpeed = 50f;
    Vector2 newPosition;
    Vector2 moveDir;
    Vector2 lookAt;
    public bool isRage = false;


    //引用：玩家对象;玩家刚体;玩家模型对象
    GameObject player;
    Rigidbody2D rig;
    GameObject model;
    public GameObject deadPlayer;
    #endregion





    #region 装备相关变量引用
    //变量：武器射击速度，射击计时器，当前玩家可拾取的武器，当前玩家手持武器是否为主武器；子弹检测层级；射线检测层级;武器位置
    public List<GameObject> pickableItems = new List<GameObject>();
    LayerMask layerMask;
    string bulletLayer;
    float weaponSpeed;
    float shootTimer = 0.02f;//射击计时器，当shootTimer大于weaponSpeed时，可以射击一次     
    bool isMainWeapon = true;
    Vector3 weaponPos;

    //引用：主武器；副武器；初始武器预制体；当前手持装备；子弹射击位置;子弹预制体
    public GameObject mainWeapon;
    public GameObject secondaryWeapon;
    public GameObject bornWeaponPrefab;
    public GameObject currentHandle;
    public GameObject bulletPrefab;
    Transform shootPos;
    #endregion






    #region UI相关变量引用
    //引用：血条填充图片，魔法值填充图片
    Image hpFillImage;
    Image magicFillImage;
    Text hpValueText;
    Text magicValueText;
    #endregion





    #region 初始化

    /// <summary>
    /// 实例化单例对象
    /// </summary>
    private void Awake()
    {
        instance = this;                                                                            //初始化单例
    }



    /// <summary>
    /// 初始化玩家相关引用：玩家对象；玩家模型对象；玩家对象刚体组件：如果玩家没有刚体组件，手动添加并置重力值为0
    /// 初始化装备相关引用：主武器对象；当前武器对象；当前武器父物体对象；当前武器位置；武器攻击速率；武器开火位置;武器位置
    /// 初始化UI相关引用：血条图片，魔法值图片
    /// </summary>
    void Start()
    {
        //玩家相关
        player = GameObject.Find("Player").gameObject;
        model = this.transform.Find("Model").gameObject;
        rig = GetComponent<Rigidbody2D>();
        if (rig == null)
        {
            rig = this.gameObject.AddComponent<Rigidbody2D>();
        }
        rig.gravityScale = 0;


        //装备相关
        mainWeapon = Instantiate(bornWeaponPrefab);
        currentHandle = mainWeapon;

        ChangeCurrentHandle();

        //UI相关:
        hpFillImage = GameObject.Find("HPFillImage").GetComponent<Image>();
        hpFillImage.fillAmount = currentHPValue / hpValue;
        hpValueText = GameObject.Find("HPValueText").GetComponent<Text>();
        hpValueText.text = Convert.ToString(currentHPValue) + "/" + Convert.ToString(hpValue);

        magicFillImage = GameObject.Find("MagicFillImage").GetComponent<Image>();
        magicFillImage.fillAmount = currentMagicValue / magicValue;
        magicValueText = GameObject.Find("MagicValueText").GetComponent<Text>();
        magicValueText.text = Convert.ToString(currentMagicValue) + "/" + Convert.ToString(magicValue);
    }
    #endregion






    #region 更新

    /// <summary>
    /// 射击方法；获取玩家下一移动方向坐标方法；玩家移动方法；玩家旋转方法；切换武器方法；拾取武器方法
    /// </summary>
    void Update()
    {
        if (shootTimer >= weaponSpeed)
        {
            Shoot();
        }
        else
        {
            shootTimer += Time.deltaTime;
        }
        BeforeMove();
        Move();
        Rotate();
        SwitchWeapon();
        PickWeapon();
        PickPotion();
    }
    #endregion






    #region 玩家移动与旋转



    /// <summary>
    /// 在角色运动前获取角色下一个运动位置新坐标
    /// </summary>
    void BeforeMove()
    {
        moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        newPosition = moveDir * moveSpeed * Time.deltaTime;
    }
    private void Move()
    {
        rig.velocity = newPosition * 100;
    }
  /// <summary>
    /// 当按下A，主角左转，按下D，主角右转
    /// 获取当前玩家对象位置，如果“当前位置只想鼠标所在位置的向量的x轴大于0，说明鼠标再右边，向右看，反之向左看
    /// </summary>
    private void Rotate()
    {
        Vector3 dir = Camera.main.WorldToScreenPoint(transform.position);
        lookAt = Input.mousePosition - dir;
        Vector2 shootDir = lookAt.normalized;
        //旋转主角与武器
        if (lookAt.x > 0)
        {
            model.transform.eulerAngles = new Vector3(0, 180, 0);
            if (shootDir.y > 0)
            {
                currentHandle.transform.eulerAngles = new Vector3(0, 0, Vector3.Angle(shootDir, new Vector2(1, 0)));
            }
            else
            {
                currentHandle.transform.eulerAngles = new Vector3(0, 0, 360 - Vector3.Angle(shootDir, new Vector2(1, 0)));
            }
        }
        else
        {
            model.transform.eulerAngles = new Vector3(0, 0, 0);
            if (shootDir.y > 0)
            {
                currentHandle.transform.eulerAngles = new Vector3(0, 180, Vector3.Angle(shootDir, new Vector2(-1, 0)));
            }
            else
            {
                currentHandle.transform.eulerAngles = new Vector3(0, 180, 360 - Vector3.Angle(shootDir, new Vector2(-1, 0)));
            }
        }
    }
    #endregion






    #region 射击
    /// <summary>
    /// 1、当玩家点击鼠标左键，开始攻击
    /// 2、从对象池（脚本）中获取子弹对象
    /// 3、设置子弹射击位置：将子弹射击位置shootPos.position赋值给子弹对象
    /// 4、设置子弹射击方向：
    ///     1、子弹射击方向就是当前游戏对象所看向的方向
    ///     2、将该方向向量（玩家对象所在位置到鼠标所在位置的向量）归一化，再判断该向量再坐标轴第几象限
    ///     3、如果射击方向在第一二象限，即归一化向量y坐标值大于0，则对子弹绕Z轴旋转“X轴正方向与射击方向的夹角”度数
    ///     4、如果射击方向在第三四象限，步骤同“3”，但需要用360°减去“步骤3”所得度数
    /// 5、获取子弹刚体组件，并为其加一个力，力的方向为射击方向，力的大小为子弹速度
    /// 6、设置子弹层级（子弹只和特定对象发生碰撞，如箱子，敌人等）
    /// </summary>
    private void Shoot()
    {
        if (Input.GetMouseButton(0))
        {
            int consumeMagic = currentHandle.GetComponent<Gun>().consumeMagic;
            if (consumeMagic <= currentMagicValue)
            {
                shootTimer = 0;
                GameObject bullet = PlayerBulletPoolManager.instance.getBullet();
                bullet.transform.position = currentHandle.transform.Find("ShootPos").position;
                Vector2 shootDir = lookAt.normalized;
                if (shootDir.y > 0)
                {
                    bullet.transform.eulerAngles = new Vector3(0, 0, Vector3.Angle(shootDir, new Vector2(1, 0)));
                }
                else
                {
                    bullet.transform.eulerAngles = new Vector3(0, 0, 360 - Vector3.Angle(shootDir, new Vector2(1, 0)));
                }
                bullet.GetComponent<Rigidbody2D>().AddForce(shootDir * Bullet.instance.speed);
                Bullet.instance.damage = currentHandle.GetComponent<Gun>().damage;  //获取武器的伤害值赋值给子弹
                currentMagicValue -= consumeMagic;

                //更新UI中玩家消耗魔法值
                updateMagic();
            }
        }
    }
    #endregion






    #region 武器相关操作



    /// <summary>
    /// 1、如果可拾取武器数量大于0并且按下E，则拾取离玩家最近的武器，即pickableWeapons[0]
    /// 2、如果副武器为空，拾取的武器赋给副武器：
    ///     1、获取副武器
    ///     2、将可拾取武器集合中移除掉拾取的武器对象
    ///     3、设置武器状态：当前武器时主武器，所以要禁用副武器
    ///     4、设置武器父物体对象
    ///     5、设置武器Tag为PlayerWeapon
    /// 3、如果副武器不为空，则判断当前武器时主武器还是副武器，将当前武器与地上的武器进行更换：
    ///     1、如果当前武器是主武器：
    ///         1、将可拾取武器的对象和位置保存起来：tempWeapon
    ///         2、将主武器父对象置为空
    ///         3、将主武器对象位置更改为“可拾取武器对象”的位置
    ///         3.1、将主武器Tag改为EnvironmentWeapon
    ///         4、将主武器对象赋值到“可拾取武器集合”中下表为0的位置，表示拾取武器后，主武器变为“离玩家最近的可拾取武器对象”
    ///         5、将临时保存的可拾取武器对象tempWeapon赋值给主武器对象
    ///         6、重新设置当前所持对象currentHandle
    ///         7、因为当前所持对象发生变化，调用ChangeCurrentHandle()方法
    ///     2、如果当前武器是副武器，同理
    /// </summary>
    private void PickWeapon()
    {
        if (pickableItems.Count > 0 && pickableItems[0].tag == "EnvironmentWeapon" && Input.GetKeyDown(KeyCode.E))
        {
            if (secondaryWeapon == null)
            {
                secondaryWeapon = pickableItems[0];
                pickableItems.RemoveAt(0);
                secondaryWeapon.SetActive(false);
                secondaryWeapon.transform.parent = this.transform;
                secondaryWeapon.gameObject.tag = "PlayerWeapon";
            }
            else if(currentHandle == mainWeapon)
            {
                
                GameObject tempWeapon = pickableItems[0];

                mainWeapon.transform.parent = null;
                mainWeapon.transform.position = pickableItems[0].transform.position;
                mainWeapon.gameObject.tag = "EnvironmentWeapon";
                pickableItems.RemoveAt(0);

                mainWeapon = tempWeapon;
                currentHandle = mainWeapon;
                ChangeCurrentHandle();
                //Debug.Log(pickableItems.Count);
            }
            else if(currentHandle == secondaryWeapon)
            {
                GameObject tempWeapon = pickableItems[0];//将当前可拾取武器保存到一个变量中

                secondaryWeapon.transform.parent = null;
                secondaryWeapon.transform.position = pickableItems[0].transform.position;
                secondaryWeapon.gameObject.tag = "EnvironmentWeapon";
                pickableItems.RemoveAt(0);

                secondaryWeapon = tempWeapon;
                currentHandle = secondaryWeapon;
                ChangeCurrentHandle();
            }
        }
    }




    /// <summary>
    /// 切换武器方法
    /// 如果副武器为空，按下Q无操作
    /// 如果副武器不位空，按下Q更换武器：如果isMainWeapon为True，即当前为主武器，则更换为副武器，反之更换为主武器
    /// 切换武器之后，还要设置：
    ///     1、武器的状态（SetActive）
    ///     2、武器的位置
    ///     3、武器的攻击速度
    ///     4、武器开火位置
    /// </summary>
    private void SwitchWeapon()
    {
        if (secondaryWeapon != null && Input.GetKeyDown(KeyCode.Q))
        {
            if (isMainWeapon)
            {
                isMainWeapon = false;
                mainWeapon.SetActive(false);
                secondaryWeapon.SetActive(true);
                currentHandle = secondaryWeapon;
                ChangeCurrentHandle();
                
            }
            else
            {
                isMainWeapon = true;
                mainWeapon.SetActive(true);
                secondaryWeapon.SetActive(false);
                currentHandle = mainWeapon;
                ChangeCurrentHandle();
            }
        }
    }




    /// <summary>
    /// 当“当前武器”发生更换时，调用的方法
    /// 判断当前物体父物体是否为Player，如果不是，更改当前武器的父物体
    /// 判断当前物体Tag是不是PlayerWeapon，如果不是，更改为PlayerWeapon
    /// 更改当前武器位置
    /// 更改当前武器速度
    /// 更改当前武器开火位置
    /// </summary>
    private void ChangeCurrentHandle()
    {
        if(currentHandle.transform.parent == null)
        {
            currentHandle.transform.parent = this.transform;
        }
        if(!currentHandle.CompareTag("PlayerWeapon"))
        {
            currentHandle.gameObject.tag = "PlayerWeapon";
        }
        currentHandle.transform.position = player.transform.position + currentHandle.GetComponent<Gun>().weaponPos;//设置武器位置
        weaponSpeed = currentHandle.GetComponent<Gun>().cooling;
        shootPos = currentHandle.transform.Find("ShootPos");
    }
    #endregion




    #region 拾取药瓶方法
    /// <summary>
    /// 如果可拾取物品数量大于0且第一个可拾取物品对象的tag为表示药瓶的Potion，则在玩家按下E键时拾取药瓶
    /// 服用血瓶：恢复50hp
    /// 服用魔法瓶：恢复50魔法值
    /// 服用恢复药瓶：恢复50hp，50魔法值
    /// 服用敏捷药瓶：移动速度提高1
    /// 服用狂暴药瓶：提高移动速度，降低武器冷却率，提高攻击力，并开启协程，在10s后取消狂暴药瓶的增幅
    /// </summary>
    private void PickPotion()
    {
        if (pickableItems.Count > 0 && pickableItems[0].tag == "Potion" && Input.GetKeyDown(KeyCode.E))
        {
            switch (pickableItems[0].GetComponent<Potion>().potionName)
            {
                case "HealthyPotion":
                    this.currentHPValue += 50;
                    if(this.currentHPValue > hpValue)
                    {
                        this.currentHPValue = hpValue;
                        updateHP();
                    }
                    else
                    {
                        updateHP();
                    }
                    Destroy(pickableItems[0].gameObject);
                    
                    break;
                case "MagicPotion":
                    this.currentMagicValue += 50;
                    if (this.currentMagicValue > magicValue)
                    {
                        this.currentMagicValue = magicValue;
                        updateMagic();
                    }
                    else
                    {
                        updateMagic();
                    }
                    Destroy(pickableItems[0].gameObject);
                    
                    break;
                case "RejuvenationPotion":
                    this.currentHPValue += 50;
                    if (this.currentHPValue > hpValue)
                    {
                        this.currentHPValue = hpValue;
                        updateHP();
                    }
                    else
                    {
                        updateHP();
                    }
                    this.currentMagicValue += 50;
                    if (this.currentMagicValue > magicValue)
                    {
                        this.currentMagicValue = magicValue;
                        updateMagic();
                    }
                    else
                    {
                        updateMagic();
                    }
                    Destroy(pickableItems[0].gameObject);
                    
                    break;
                case "AgilityPotion":
                    this.moveSpeed += 1;
                    Destroy(pickableItems[0].gameObject);
                    
                    break;
                case "RagePotion":
                    isRage = true;
                    this.moveSpeed += 5;
                    this.currentHandle.GetComponent<Gun>().cooling -= 0.1f;
                    this.currentHandle.GetComponent<Gun>().damage += 999;
                    StartCoroutine("Calm");
                    Destroy(pickableItems[0].gameObject);
                    
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 狂暴药瓶失效协程方法
    /// </summary>
    /// <returns></returns>
    IEnumerator Calm()
    {
        //等10秒
        yield return new WaitForSeconds(10f);
        isRage = false;
        this.moveSpeed -= 5;
        this.currentHandle.GetComponent<Gun>().cooling += 0.1f;
        this.currentHandle.GetComponent<Gun>().damage -= 999;
    }
    #endregion






    #region 减血与死亡方法
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "EnemyBullet")
        {
            
            currentHPValue -= collision.gameObject.GetComponent<Bullet>().damage;
            Destroy(collision.gameObject);
            //更新UI中玩家消耗血量
            if (currentHPValue <= 0)//玩家死亡
            {
                List<GameObject> enemies = new List<GameObject>();
                GameObject.FindGameObjectsWithTag("Enemy");
                foreach(GameObject enemy in enemies)
                {
                    enemy.GetComponent<Enemy>().enemyState = Enemy.EnemyState.PATROL;
                }
                currentHPValue = 0;
                hpFillImage.fillAmount = 0;
                hpValueText.text = "0/100";
                Instantiate(deadPlayer, this.transform.position, this.transform.rotation);
                Destroy(this.gameObject);
                Invoke("ReturnToMenu", 5);//游戏失败5s后跳转到菜单界面
            }
            else
            {
                updateHP();
            }
        }
    }
    /// <summary>
    /// 玩家死亡，跳转到菜单
    /// </summary>
    private void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
    #endregion

    #region 更新血量魔法值UI方法
    private void updateHP()
    {
        hpFillImage.fillAmount = (float)Math.Round((decimal)currentHPValue / hpValue, 2);//相除保留两位小数
        hpValueText.text = Convert.ToString(currentHPValue) + "/" + Convert.ToString(hpValue);
    }

    private void updateMagic()
    {
        magicFillImage.fillAmount = (float)Math.Round((decimal)currentMagicValue / magicValue, 2);//相除保留两位小数
        magicValueText.text = Convert.ToString(currentMagicValue) + "/" + Convert.ToString(magicValue);
    }
    #endregion
}
