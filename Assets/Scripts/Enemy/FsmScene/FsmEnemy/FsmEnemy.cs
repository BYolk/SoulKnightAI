using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 开启敌人的有限状态机
/// </summary>
public class FsmEnemy : MonoBehaviour
{
    public Fusm fusm;//模糊状态机

    //敌人的有限状态机
    public Fsm fsm;
    //单例
    public FsmEnemy fsmEnemy;




    /// <summary>
    /// 敌人状态
    /// </summary>
    public enum FsmEnemyStateEnum
    {
        PatrolState,
        AttackState,
        DeadState,
        EscapeState,
        HideState,
        PickState,
        RunAwayState
    }
    public GameObject[] fsmEnemyPrefabs;//所有怪物的预制体









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
    public bool isRage = false;



    //引用：怪物对象;怪物刚体;怪物模型对象
    [HideInInspector]
    public Rigidbody2D rig;
    [HideInInspector]
    public Transform target;//敌人攻击目标，即玩家
    #endregion









    #region 攻击相关变量引用
    //变量：射击速度，射击计时器，当前玩家可拾取的武器，当前玩家手持武器是否为主武器；子弹检测层级；射线检测层级;武器位置
    public List<GameObject> pickableItems = new List<GameObject>();


    //引用：主武器；副武器；初始武器预制体；当前手持装备；子弹射击位置;子弹预制体
    public Transform shootPos;
    public int lowestLevelDamage = 5;    
    public float lowestLevelShootSpeed = 1f;
    #endregion







    #region 怪物旋转相关变量
    public float horizontal;
    public float vertical;
    #endregion







    #region 初始化
    private void Start()
    {
        fsmEnemy = this;
        //初始化当前游戏对象的刚体组件与模型对象
        rig = this.GetComponent<Rigidbody2D>();
        target = GameObject.Find("Player").transform;
        shootPos = this.transform.Find("Model").transform.Find("ShootPos").transform;

        Debug.Log("初始化有限状态机");
        fsm = new Fsm(fsmEnemy);//实例化敌人的有限状态机
        fsm.SetEnemyState(new EscapeState(fsm));//默认进入巡逻状态



        #region 模糊状态机相关代码
        fusm = new Fusm(fsmEnemy);//实例化敌人模糊状态机
        fusm.AddState(new HealthyState(fusm));
        fusm.AddState(new DyingState(fusm));
        fusm.AddState(new InjuredState(fusm));
        #endregion
    }
    #endregion








    #region 更新
    void Update()
	{
        fsm.Update();
        fusm.Update();
    }
    #endregion







    #region 碰撞处理：减血方法、撞墙改变移动方向方法
    /// <summary>
    /// 如果敌人和玩家子弹发生碰撞，减少敌人血量，如果敌人血量低于0，则敌人状态设置为死亡
    /// 如果敌人和墙发生碰撞，获取敌人当前移动方向并改变敌人方向
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "PlayerBullet")
        {
            Debug.Log("受到攻击");
            currentHPValue -= collision.gameObject.GetComponent<Bullet>().damage;
            if (currentHPValue <= 0)
            {
                currentHPValue = 0;
                fsm.SetEnemyState(new DeadState(fsm));//死亡，直接销毁对象
            }
            else if((float)Math.Round((decimal)currentHPValue / hpValue, 2) < 0.1)//(float)Math.Round((decimal)currentHPValue / hpValue, 2)：保留两位小数
            {
                Debug.Log("血量不足百分之10");
                fsm.SetEnemyState(new RunAwayState(fsm));//进入逃跑状态
                EnemyPerspective.instance.isInRunAwayState = true;
            }
            Debug.Log("当前血量为" + currentHPValue.ToString());

            //模糊状态机
            List<FusmBaseState> states = fusm.states;//获取模糊状态机所有状态
            float hpPercentage = (float)Math.Round((decimal)currentHPValue / hpValue, 2);//得到当前血量百分比
            Debug.Log("当前血量百分比为" + hpPercentage);
            foreach (FusmBaseState state in fusm.states)
            {
                state.Evaluate();//计算模糊状态机的激活等级
            }
            fusm.UpdateFusm();//计算激活等级后对模糊状态机进行更新
        }



        else if ((collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Boxes" || collision.gameObject.tag == "Obstacle") && fsm.GetEnemyState().GetType().Name == "PatrolState")
        {
            Debug.Log("撞墙，改变方向");
            PatrolState.EnemyHitWallChangeDir();
        }


        //如果碰到障碍物并且处于逃跑状态，调用 ANNController 脚本对象的 Death 方法
        else if ((collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Boxes" || collision.gameObject.tag == "Obstacle") && fsm.GetEnemyState().GetType().Name == "EscapeState")
        {
            ANNController controller = transform.GetComponent<ANNController>();
            //Debug.Log("死亡前神经网络的适应度为：" + controller.overallFitness.ToString());
            controller.Death();
            Debug.Log("进入逃脱状态");
        }

        //如果逃跑状态下碰到了传送门，则表示当前神经网络是“合格”的神经网络
        else if (collision.gameObject.tag == "Door" && fsm.GetEnemyState().GetType().Name == "EscapeState")
        {
            Debug.Log("成功逃脱");
            GameObject.Find("Empty_GeneticManager").GetComponent<GeneticManager>().exceptSuccessed += 1;
            ANNController controller = transform.GetComponent<ANNController>();
            controller.Death();
            Debug.Log("进入逃脱状态");
        }
    }
    #endregion








    #region 敌人死亡处理
    public void Dead()
    {
        Destroy(this.gameObject);
    }
    #endregion







    #region 删除可拾取列表里面的游戏对象
    public void DestroyItem(GameObject item)
    {
        Destroy(item);
    }
    #endregion








    #region 在状态类中开启协程方法
    public void StartCoroutineInState(string coroutineName)
    {
        StartCoroutine(coroutineName);
    }





    /// <summary>
    /// 狂暴药瓶失效协程方法
    /// </summary>
    /// <returns></returns>
    IEnumerator Calm()
    {
        Debug.Log("开启敌人“冷静”协程");
        //等10秒
        yield return new WaitForSeconds(10f);
        isRage = false;
        this.moveSpeed -= 5;
        this.shootSpeed += 0.1f;
        this.damage -= 10;
        Debug.Log("执行完毕");
    }
    #endregion


}
