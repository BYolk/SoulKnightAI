using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAwayState : EnemyBaseState
{
    #region 有限状态机对象与开启有限状态机的脚本对象
    private Fsm fsm;//获取敌人有限状态机对象
    private FsmEnemy fsmEnemy;//获取敌人对象脚本对象
    #endregion

    #region 定义逃跑过程中需要用到的变量和引用
    Rigidbody2D rig;//敌人的刚体
    GameObject bush;//草丛
    Vector3 dir;//敌人到草从方向
    float moveSpeed;
   
    #endregion

    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="fsmEnemy">构造攻击状态时，需要传递一个敌人的脚本对象FsmEnemy</param>
    public RunAwayState(Fsm fsm)
    {
        Debug.Log("进入逃跑状态");
        this.fsm = fsm;
        this.fsmEnemy = fsm.GetFsmEnemy();

        moveSpeed = fsmEnemy.moveSpeed;
        moveSpeed += 5;//逃跑状态速度加5
        this.rig = fsmEnemy.rig;
        this.bush = ItemManager.instance.bush;
        
    }
    #endregion

    #region 更新
    //Handle和Update方法都是在StartFsm的Update方法中调用，即Handle和Update方法都相当于Unity的Update方法，每帧调用一次
    public void Handle()
    {
        RunAway();
    }
    public void Update()
    {

    }
    #endregion

    private void RunAway()
    {
        dir = bush.transform.position - fsmEnemy.transform.position;
        rig.velocity = dir * Time.deltaTime * moveSpeed * 100;//往草丛方向走
    }
}
