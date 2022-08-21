using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 模糊状态机
/// </summary>
public class Fusm 
{
    FsmEnemy fsmEnemy;//敌人对象
    public List<FusmBaseState> states;//模糊状态机的所有状态集合
    public List<FusmBaseState> activated;//模糊状态机的已经激活的状态集合
    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    public Fusm(FsmEnemy fsmEnemy)
    {
        this.fsmEnemy = fsmEnemy;
        states = new List<FusmBaseState>();//创建模糊状态机所有状态集合变量
        activated = new List<FusmBaseState>();//创建模糊状态机已激活状态集合变量
    }
    #endregion
    /// <summary>
    /// 更新模糊状态机，获取所有已激活状态。该方法会在FsmEnemy NPC游戏对象的Update方法中每帧调用
    /// 清除已经激活的状态机，定义未激活状态集合。计算激活因子，若激活因子达到要求则将该状态添加到激活状态及变量，反之将状态添加到未激活状态变量
    /// 清除所有未激活状态
    /// </summary>
    public void UpdateFusm()
    {
        if (states.Count == 0)
            return;
        activated.Clear();
        List<FusmBaseState> nonActiveStates = new List<FusmBaseState>();
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i].Evaluate() > 0)
                activated.Add(states[i]);
            else
                nonActiveStates.Add(states[i]);   
        }
        if (nonActiveStates.Count != 0)
        {
            for (int i = 0; i < nonActiveStates.Count; i++)
                nonActiveStates[i].Exit();
        }
    }
    public void Update()// 调用状态行为方法。此Update方法在FsmEnemy的Update方法中调用，即此处的Update方法相当于Unity的Update方法
    {
        foreach(FusmBaseState activatedState in activated)//遍历每一个已经激活的状态，并调用该状态的Update方法
        {
            activatedState.Update();
        }
    }



    #region 添加状态
    /// <summary>
    /// 添加状态
    /// </summary>
    /// <param name="newState"></param>
    public void AddState(FusmBaseState newState)
    {
        states.Add(newState);
    }
    #endregion

    #region 判断状态是否被激活
    /// <summary>
    /// 判断某一个状态是否激活：遍历已激活的状态集合，并判断与给定的state是否相等
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool IsActive(FusmBaseState state)
    {
        if (activated.Count != 0)
        {
            for (int i = 0; i < activated.Count; i++)
            {
                if (activated[i] == state)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region 重置模糊状态机
    /// <summary>
    /// 重置模糊状态机：遍历所有状态对象，清除并初始化
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < states.Count; i++)
        {
            states[i].Exit();
            states[i].Init();
        }
    }
    #endregion

    #region 获取NPC游戏对象
    public FsmEnemy GetFusmEnemy()
    {
        return fsmEnemy;
    }
    #endregion

    #region 获取敌人基本状态对象或状态名称
    /// <summary>
    /// 获取基本状态对象
    /// </summary>
    /// <returns></returns>
    public List<FusmBaseState> GetEnemyStates()
    {
        return states;
    }


    #endregion

}
