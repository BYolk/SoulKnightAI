using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : EnemyBaseState
{
    #region 有限状态机对象与开启有限状态机的脚本对象
    private Fsm fsm;//获取敌人有限状态机对象
    private FsmEnemy fsmEnemy;//获取敌人对象脚本对象
    #endregion
    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="fsmEnemy">构造死亡状态时，需要传递一个敌人的脚本对象FsmEnemy</param>
    public DeadState(Fsm fsm)
    {
        Debug.Log("进入死亡状态");
        this.fsm = fsm;
        this.fsmEnemy = fsm.GetFsmEnemy();
    }
    #endregion
    #region 更新
    //Handle和Update方法都是在StartFsm的Update方法中调用，即Handle和Update方法都相当于Unity的Update方法，每帧调用一次
    public void Handle()
    {
        Dead();
    }
    public void Update()
    {
    }
    #endregion
    #region 死亡方法
    private void Dead()
    {
        fsmEnemy.Dead();
    }
    #endregion
}
