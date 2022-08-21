using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideState : EnemyBaseState
{
    #region 有限状态机对象与开启有限状态机的脚本对象
    private Fsm fsm;//获取敌人有限状态机对象
    private FsmEnemy fsmEnemy;//获取敌人对象脚本对象
    #endregion
    #region 定义逃跑过程中需要用到的变量和引用
    int currentHPValue;
    int currentMagicValue;
    float timer = 0f;
    GameObject bush;
    #endregion
    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="fsmEnemy">构造攻击状态时，需要传递一个敌人的脚本对象FsmEnemy</param>
    public HideState(Fsm fsm)
    {
        Debug.Log("进入隐藏状态");
        this.fsm = fsm;
        this.fsmEnemy = fsm.GetFsmEnemy();


        currentHPValue = fsmEnemy.currentHPValue;
        currentMagicValue = fsmEnemy.currentMagicValue;
        bush = ItemManager.instance.bush;
    }
    #endregion
    #region 更新
    //Handle和Update方法都是在StartFsm的Update方法中调用，即Handle和Update方法都相当于Unity的Update方法，每帧调用一次
    public void Handle()
    {
        HideInBush();
    }
    public void Update()
    {

    }
    #endregion
    private void HideInBush()
    {
        fsmEnemy.transform.position = bush.transform.position;
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            if(currentHPValue < 100)
            {
                currentHPValue += 1;
            }
            if(currentMagicValue < 100)
            {
                currentMagicValue += 1;
            }
            Debug.Log("敌人的血量" + currentHPValue.ToString());
            Debug.Log("敌人的魔法值" + currentMagicValue.ToString());
            fsmEnemy.currentHPValue = currentHPValue;
            fsmEnemy.currentMagicValue = currentMagicValue;
            timer = 0f;
        }
    }
}
