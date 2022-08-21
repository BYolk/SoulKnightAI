using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 垂死状态类：可能同时存在垂死状态和受伤状态
/// </summary>
public class DyingState : FusmBaseState
{
    #region

    #endregion
    #region 模糊状态机对象与开启模糊有限状态机的脚本对象
    private Fsm fsm;//获取敌人有限状态机对象
    private Fusm fusm;//获取敌人模糊状态机对象
    private FsmEnemy fsmEnemy;//获取敌人对象脚本对象
    #endregion

    private float activation;//隶属于本状态的程度，0.0f表示完全不属于此状态，1.0f表示完全属于此状态
    int hpValue;//血量上限
    int currentHPValue;//当前血量
    float timer;//定时器
    float hpPercentage;//血量百分比


    public DyingState(Fusm fusm)
    {
        //Debug.Log("注册垂死状态");
        this.fusm = fusm;
        this.fsmEnemy = fusm.GetFusmEnemy();


    }


    public void Update()
    {
        
    }





    #region 计算激活因子
    /// <summary> 
    /// 在 Fusm 模糊状态机的 UpdateFusm 更新模糊状态机方法中调用 Evaluate ，判断该状态的 activation 是否大于0
    /// 如果血量百分比为百分之四十，说明完完全全处于受伤状态
    /// 如果血量百分比大于百分之二十小于百分之四十，说明同时处于受伤状态与垂死状态
    /// 如果血量百分比大于百分之四十小于百分之八十，说明同时处于受伤状态与健康状态
    /// </summary>
    public float Evaluate()
    {
        hpValue = fsmEnemy.hpValue;
        currentHPValue = fsmEnemy.currentHPValue;
        float hpPercentage = (float)Math.Round((decimal)currentHPValue / hpValue, 2);//得到当前血量百分比
        activation = -0.025f * (hpPercentage * 100) + 1;
        CheckBounds();//检测 activation 的合法性


        if (activation > 0)
        {
            Debug.Log("当前处于垂死状态，隶属于垂死状态的程度为：" + activation.ToString());
        }
        if (activation > 0.34)
        {
            Debug.Log("当前血量隶属于垂死状态的程度更高，切换到逃跑状态");
            //fsm.SetEnemyState(new RunAwayState(fsm));
            //fsmEnemy.GetComponent<EnemyPerspective>().isInRunAwayState = true;
        }
        else if(activation > 0 && activation <= 0.34)
        {
            Debug.Log("当前血量隶属于受伤状态的程度更高，如果场景中有治愈药瓶，进入拾取状态");
            /*GameObject[] potions = ItemManager.instance.potions;
            foreach(GameObject potion in potions)//获取场景中所有药瓶，遍历药瓶，如果药瓶能恢复HP，则进入拾取状态
            {
                if (potion && (potion.GetType().Name == "HealthyPotion" || potion.GetType().Name == "RejuvenationPotion"))
                {
                    fsm.SetEnemyState(new PickState(fsm, potion));
                    //fsmEnemy.GetComponent<EnemyPerspective>().isInRunAwayState = true;
                    break;
                }
            }*/
        }

        return activation;//返回 activation
    }
    #endregion





    #region 检测激活因子值的合法性
    /// <summary>
    /// 检测激活因子值的合法性
    /// </summary>
    /// <param name="lowerBound"></param>
    /// <param name="upperBound"></param>
    public void CheckBounds(float lowerBound = 0, float upperBound = 1)
    {
        CheckLowerBound(lowerBound);
        CheckUpperBound(upperBound);
    }
    /// <summary>
    /// 激活因子下限合法性
    /// </summary>
    /// <param name="lowerBound"></param>
    public void CheckLowerBound(float lowerBound = 0)
    {
        if (activation < lowerBound)
        {
            activation = lowerBound;
        }
    }
    /// <summary>
    /// 激活因子上限合法性
    /// </summary>
    /// <param name="upperBound"></param>
    public void CheckUpperBound(float upperBound = 1)
    {
        if (activation > upperBound)
        {
            activation = upperBound;
        }

    }
    #endregion
























    public void Enter()
    {

    }

    public void Exit()
    {

    }

    public void Init()
    {

    }

}
