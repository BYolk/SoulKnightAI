using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : EnemyBaseState
{
    #region 有限状态机对象与开启有限状态机的脚本对象
    private Fsm fsm;//获取敌人有限状态机对象
    private FsmEnemy fsmEnemy;//获取敌人对象脚本对象
    #endregion

    #region 敌人旋转移动相关变量引用
    //变量
    float shootTimer = 0;//射击计时器
    float shootSpeed;//射击速度
    int magicValue;//总魔法值
    int currentMagicValue;//当前魔法值
    int damage;//伤害
    int consumeMagic;//消耗魔法值
    int lowestLevelDamage = 5;//最低伤害值（当魔法值不足时，使用最低伤害）
    //float lowestLevelShootSpeed = 1;//最低伤害值（当魔法值不足时，使用最低射速）

    //引用
    Transform shootPos;//射击位置
    Transform target;//攻击目标Transform组件
    static Transform enemyTransform;//敌人Transform组件
    #endregion

    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="fsmEnemy">构造攻击状态时，需要传递一个敌人的脚本对象FsmEnemy</param>
    public AttackState(Fsm fsm)
    {
        Debug.Log("进入攻击状态");
        this.fsm = fsm;
        this.fsmEnemy = fsm.GetFsmEnemy();

        shootSpeed = fsmEnemy.shootSpeed;
        magicValue = fsmEnemy.magicValue;
        currentMagicValue = fsmEnemy.currentMagicValue;
        damage = fsmEnemy.damage;
        consumeMagic = fsmEnemy.consumeMagic;
        lowestLevelDamage = fsmEnemy.lowestLevelDamage;
        enemyTransform = fsmEnemy.transform;
        shootPos = fsmEnemy.shootPos;
        target = fsmEnemy.target;  
    }
    #endregion

    #region 更新
    //Handle和Update方法都是在StartFsm的Update方法中调用，即Handle和Update方法都相当于Unity的Update方法，每帧调用一次
    public void Handle()
    {
        //FindTarget();//查找玩家---该方法在感知系统内实现
    }
    public void Update()
    {
        if (shootTimer >= shootSpeed)
        {
            Attack();
        }
        else
        {
            shootTimer += Time.deltaTime;
        }
    }
    #endregion

    #region 寻找玩家方法
    /// <summary>
    /// 敌人攻击状态寻找主角方法:在攻击状态下找不到主角进入巡逻状态
    /// </summary>
    private void FindTarget()
    {
        Vector3 pos = target.position;
        float angle = 0;
        if (enemyTransform.rotation.y == 180)//如果怪物朝右看
        {
            angle = Vector3.Angle(-enemyTransform.right, pos - enemyTransform.position);//敌人自身向前向量与敌人和玩家向量的夹角
        }
        else if (enemyTransform.rotation.y == 0)//如果怪物朝坐看
        {
            angle = Vector3.Angle(-enemyTransform.right, pos - enemyTransform.position);
        }

        if (!(angle < 120 && Vector3.Distance(enemyTransform.position, target.position) < 20))//当角度小于120°，距离小于20，说明目标不在敌人视觉范围
        {
            fsm.SetEnemyState(new PatrolState(fsm));//改变状态为巡逻状态
        }
        else
        {
            //获取玩家当前手持武器等级与敌人自身等级，如果两者相差超过3，则敌人进入逃跑状态
            Player playerScript = GameObject.Find("Player").GetComponent<Player>();//获取玩家Player脚本组件
            int gunLevel = playerScript.currentHandle.gameObject.GetComponent<Gun>().level;
            int enemyLevel = enemyTransform.gameObject.GetComponent<FsmEnemy>().level;

            //如果玩家处于狂暴状态，则敌人进入逃跑状态
            bool playerIsRage = playerScript.isRage;
            if (gunLevel - enemyLevel >= 3 || playerIsRage == true)
            {
                fsm.SetEnemyState(new EscapeState(fsm));//改变状态为逃跑状态
            }
        }
    }
    #endregion

    #region 攻击相关方法
    /// <summary>
    /// 只有玩家死亡或敌人死亡，敌人才会从攻击状态切换到巡逻状态或者死亡状态
    /// 如果敌人攻击耗蓝值小于当前魔法值，则进行攻击，反之将敌人攻击模式切换到与等级为0的武器相同。
    /// </summary>
    private void Attack()
    {
        shootTimer = 0;
        Vector3 shootDir = (target.position - enemyTransform.position).normalized;
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
}
