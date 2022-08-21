using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物没有武器，但是怪物可以通过武器增加自身等级
/// </summary>
public class Enemy : MonoBehaviour
{
    public static Enemy instance;






    #region 怪物属性相关变量与引用
    //变量：血量，魔法值，移动速度，怪物等级,攻击力，耗蓝
    public int hpValue = 100;
    public int magicValue = 100;
    public int currentHPValue = 100;
    public int currentMagicValue = 100;
    public int damage = 5;
    public int consumeMagic = 0;
    public float moveSpeed = 5;
    public float shootSpeed = 1f;
    public int level = 0;
    


    //引用：怪物对象;怪物刚体;怪物模型对象
    GameObject enemy;
    Rigidbody2D rig;
    GameObject model;
    public EnemyState enemyState;//敌人状态状态
    public Transform target;//敌人攻击目标，即玩家
    #endregion





    #region 攻击相关变量引用
    //变量：射击速度，射击计时器，当前玩家可拾取的武器，当前玩家手持武器是否为主武器；子弹检测层级；射线检测层级;武器位置
    public List<GameObject> pickableItems = new List<GameObject>();
    float shootTimer = 0.02f;//射击计时器，当shootTimer大于shootSpeed时，可以射击一次     


    //引用：主武器；副武器；初始武器预制体；当前手持装备；子弹射击位置;子弹预制体
    Transform shootPos;
    int lowestLevelDamage = 5;
    #endregion



    #region 敌人移动旋转相关变量与引用
    //变量：敌人转向计时器；敌人往左方向变量；敌人往右方向变量
    private float timeValChangeDirection = 0;
    private float horizontal;
    private float vertical;
    #endregion

    /// <summary>
    /// 敌人状态
    /// </summary>
    public enum EnemyState
    {
        PATROL,
        ATTACK,
        DEAD,
    }

    #region 初始化
    private void Start()
    {
        instance = this;
        rig = this.GetComponent<Rigidbody2D>();
        enemyState = EnemyState.PATROL;
        target = GameObject.Find("Player").transform;
        shootPos = this.transform.Find("Model").transform.Find("ShootPos").transform;
    }
    #endregion


    #region 更新
    private void Update()
    {
        FindTarget();
        switch (enemyState)//在BOSS不同状态调用不同状态方法
        {
            case EnemyState.PATROL:
                Patrol();
                break;
            case EnemyState.ATTACK:
                if (shootTimer >= shootSpeed)
                {
                    Attack();
                }
                else
                {
                    shootTimer += Time.deltaTime;
                }
                break;
            case EnemyState.DEAD:
                Dead();
                break;
            default:
                break;
        }
    }
    #endregion







    #region 巡逻方法
    /// <summary>
    /// 敌人默认为巡逻状态，根据预先确定好的敌人路径巡逻，当玩家进入敌人视度范围，切换巡逻状态为攻击状态
    /// </summary>
    private void Patrol()
    {
        Move();
    }

    /// <summary>
    /// 敌人巡逻状态的移动方法
    /// </summary>
    private void Move()
    {
        if (timeValChangeDirection >= 4)
        {
            // 转向时间计时器到达4秒
            int num = Random.Range(0, 8);
            if (num >= 5)
            {
                //往下走，概率为八分之3
                vertical = -1;
                horizontal = 0;
            }
            else if (num == 0)
            {
                //往上走，概率为八分之一
                vertical = 1;
                horizontal = 0;
            }
            else if (num > 0 && num <= 2)
            {
                //向左走
                horizontal = -1;
                vertical = 0;
            }
            else if (num > 2 && num <= 4)
            {
                //向右走
                horizontal = 1;
                vertical = 0;
            }
            //旋转后timeValChangeDirection要归零
            timeValChangeDirection = 0;
        }
        else
        {
            //转向时间计时器还未到达4秒
            timeValChangeDirection += Time.deltaTime;
        }

        //向左或者向右旋转
        if (horizontal < 0)
        {
            this.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (horizontal > 0)
        {
            this.transform.eulerAngles = new Vector3(0, 180, 0);
        }
        rig.velocity = new Vector2(horizontal, vertical) * Time.deltaTime * moveSpeed * 100;
    }
    
    


    /// <summary>
    /// 敌人巡逻状态寻找主角方法:
    /// 获取主角位置坐标，计算主角和当前敌人所看方向夹角
    /// </summary>
    private void FindTarget()
    {  
        Vector3 pos = target.position;
        float angle = 0;
        if(this.transform.rotation.y == 180)//如果怪物朝右看
        {
            
            angle = Vector3.Angle(-this.transform.right, pos - this.transform.position);//敌人自身向前向量与敌人和玩家向量的夹角
        }
        else if(this.transform.rotation.y == 0)//如果怪物朝坐看
        {
            angle = Vector3.Angle(-this.transform.right, pos - this.transform.position);
        }
        
        if (angle < 60 && Vector3.Distance(this.transform.position, target.position) < 10)//当角度小于60°，距离小于10，说明目标进入敌人视觉范围
        {
            enemyState = EnemyState.ATTACK;//更改敌人状态为攻击状态
        }
        else
        {
            Debug.Log("巡逻状态");
            enemyState = EnemyState.PATROL;
        }
        
    }
    #endregion








    /// <summary>
    /// 只有玩家死亡或敌人死亡，敌人才会从攻击状态切换到巡逻状态或者死亡状态
    /// 如果敌人攻击耗蓝值小于当前魔法值，则进行攻击，反之将敌人攻击模式切换到与等级为0的武器相同。
    /// </summary>
    #region 攻击方法
    private void Attack()
    {
        shootTimer = 0;
        Vector3 shootDir = (target.position - this.transform.position).normalized;
        if (consumeMagic <= currentMagicValue)//如果敌人耗蓝值够用，使用当前武器
        {
            GetBullet(shootDir);
            Bullet.instance.damage = damage;  //获取自身伤害值赋值给子弹
            currentMagicValue -= consumeMagic;
        }
        else//如果敌人耗蓝值不够，使用初级武器
        {
            GetBullet(shootDir);
            Bullet.instance.damage = lowestLevelDamage;  //获取自身伤害值赋值给子弹
        }

    }

    /// <summary>
    /// 获取子弹方法
    /// </summary>
    /// <param name="shootDir">子弹要射击的方向</param>
    private void GetBullet(Vector3 shootDir)
    {
        GameObject bullet = EnemyBulletPoolManager.instance.getBullet();
        bullet.transform.position = shootPos.position;
        if (shootDir.y > 0)
        {
            bullet.transform.eulerAngles = new Vector3(0, 0, Vector3.Angle(shootDir, new Vector2(1, 0)));
        }
        else
        {
            bullet.transform.eulerAngles = new Vector3(0, 0, 360 - Vector3.Angle(shootDir, new Vector2(1, 0)));
        }
        bullet.GetComponent<Rigidbody2D>().AddForce(shootDir * Bullet.instance.speed);
    }
    #endregion




    #region 死亡方法
    /// <summary>
    /// 当敌人血量低于或等于0，切换敌人状态为死亡状态
    /// </summary>
    private void Dead()
    {
        Destroy(this.gameObject);
    }
    #endregion
    #region 减血方法与撞墙改变移动方向方法
    /// <summary>
    /// 如果敌人和玩家子弹发生碰撞，减少敌人血量，如果敌人血量低于0，则敌人状态设置为死亡
    /// 如果敌人和墙发生碰撞，获取敌人当前移动方向并改变敌人方向
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "PlayerBullet")
        {
            currentHPValue -= collision.gameObject.GetComponent<Bullet>().damage;
            if(currentHPValue <= 0)
            {
                currentHPValue = 0;
                enemyState = EnemyState.DEAD;
            }
        }
        else if(collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Boxes" || collision.gameObject.tag == "Obstacle")
        {
            EnemyHitWallChangeDir();
        }
    }

    /// <summary>
    /// 如果敌人撞墙，改变敌人方向
    /// </summary>
    private void EnemyHitWallChangeDir()
    {
        int num = Random.Range(0, 3);
        if (horizontal > 0)//如果敌人在向右过程撞墙，随机转向
        {
            if (num == 0)
            {
                horizontal = -1;
            }
            else if (num == 1)
            {
                horizontal = 0;
                vertical = 1;
            }
            else
            {
                horizontal = 0;
                vertical = -1;
            }

        }
        else if (horizontal < 0)//如果敌人在向左过程撞墙，随机转向
        {
            if (num == 0)
            {
                horizontal = 1;
            }
            else if (num == 1)
            {
                horizontal = 0;
                vertical = 1;
            }
            else
            {
                horizontal = 0;
                vertical = -1;
            }
        }
        else if (vertical > 0)//如果敌人在向上过程撞墙，随机转向
        {
            if (num == 0)
            {
                vertical = -1;
            }
            else if (num == 1)
            {
                horizontal = 1;
                vertical = 0;
            }
            else
            {
                horizontal = -1;
                vertical = 0;
            }
        }
        else if (vertical < 0)//如果敌人在向下过程撞墙，随机转向
        {
            if (num == 0)
            {
                vertical = 1;
            }
            else if (num == 1)
            {
                horizontal = 1;
                vertical = 0;
            }
            else
            {
                horizontal = -1;
                vertical = 0;
            }
        }

        //根据敌人方向旋转敌人
        if (horizontal < 0)
        {
            this.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (horizontal > 0)
        {
            this.transform.eulerAngles = new Vector3(0, 180, 0);
        }
        rig.velocity = new Vector2(horizontal, vertical) * Time.deltaTime * moveSpeed * 100;
        timeValChangeDirection = 0;
    }
    #endregion
}
