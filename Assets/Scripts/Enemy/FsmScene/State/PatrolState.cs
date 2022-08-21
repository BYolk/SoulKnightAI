using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 巡逻状态：在巡逻状态下寻找玩家，如果识别到玩家，判断玩家威胁等级：
///     1、如果玩家武器比自身等级高出3，则进入逃跑状态，反之进入攻击状态
///     2、如果玩家处于Rage状态(Player.cs脚本的Rage变量为True），则进入逃跑状态，反之进入攻击状态
/// </summary>
public class PatrolState : EnemyBaseState
{
    #region 有限状态机对象与开启有限状态机的脚本对象
    private Fsm fsm;//获取敌人有限状态机对象
    private FsmEnemy fsmEnemy;//获取敌人对象脚本对象
    #endregion

    #region 敌人旋转移动相关变量引用
    //变量
    static float timeValChangeDirection;//转向计时器
    static float vertical;//用于判断敌人上下方向旋转
    static float horizontal;//用于判断敌人左右方向旋转
    static float moveSpeed;//敌人移动速度

    //引用
    static Rigidbody2D rig;
    Transform target;
    static Transform enemyTransform;
    #endregion

    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="fsmEnemy">构造巡逻状态时，需要传递一个敌人的脚本对象FsmEnemy</param>
    public PatrolState(Fsm fsm)
    {
        Debug.Log("进入巡逻状态");
        this.fsm = fsm;
        this.fsmEnemy = fsm.GetFsmEnemy();
        moveSpeed = fsmEnemy.moveSpeed;
        rig = fsmEnemy.rig;
        target = fsmEnemy.target;
        enemyTransform = fsmEnemy.transform;
    }
    #endregion

    #region 更新
    //Handle和Update方法都是在StartFsm的Update方法中调用，即Handle和Update方法都相当于Unity的Update方法，每帧调用一次
    public void Handle()
    {
        //FindTarget();//查找玩家---该方法在感知系统内实现
        Patrol();//巡逻
    }
    public void Update()
    {
        timeValChangeDirection += Time.deltaTime;
    }
    #endregion

    #region 寻找玩家方法
    /// <summary>
    /// 敌人巡逻状态寻找主角方法:在巡逻状态下找到主角进入攻击状态
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

        if (angle < 120 && Vector3.Distance(enemyTransform.position, target.position) < 20)//当角度小于120°，距离小于20，说明目标进入敌人视觉范围
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
            else
            {
                fsm.SetEnemyState(new AttackState(fsm));//改变状态为攻击状态
            }
        }
    }
    #endregion

    #region 巡逻方法
    /// <summary>
    /// 敌人巡逻方法
    /// </summary>
    private void Patrol()
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
            enemyTransform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (horizontal > 0)
        {
            enemyTransform.eulerAngles = new Vector3(0, 180, 0);
        }
        rig.velocity = new Vector2(horizontal, vertical) * Time.deltaTime * moveSpeed * 100;
    }
    #endregion

    #region 撞墙改变运动方法方法
    /// <summary>
    /// 如果敌人撞墙，改变敌人方向
    /// </summary>
    public static void EnemyHitWallChangeDir()
    {
        int num = Random.Range(0, 3);
        if (horizontal > 0)//如果敌人在向右过程撞墙0，随机转向
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
            enemyTransform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (horizontal > 0)
        {
            enemyTransform.eulerAngles = new Vector3(0, 180, 0);
        }
        rig.velocity = new Vector2(horizontal, vertical) * Time.deltaTime * moveSpeed * 100;
        timeValChangeDirection = 0;
    }
    #endregion
}
