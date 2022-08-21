using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 应用FSM有限状态机的敌人脚本
/// 
/// 1、状态模式：
///     为所有状态定义一个接口 StateStructure（该接口不仅仅可以应用与敌人状态，还可以拓展到任何游戏实体状态）
///     为每个状态定义一个类（为Enemy的每一个状态都定义一个类）
///     进行类的委托
///     
/// 2、状态模式结构：
///     1、State接口：为所有状态定义的接口
///     2、具体状态类：为每个状态定义一个类
///         ConcreteStateA：具体状态A
///         ConcreteStateB：具体状态B
///         ……
///     
///     3、Context接口：
///         1、上下文接口，用于管理状态，即状态管理器
///         2、在Context中定义相关接口方法
///         3、定义当前状态（当前状态即为具体状态类的一个实例）
/// </summary>
public class Fsm
{
    #region 变量引用
    EnemyBaseState state;//敌人基本状态变量，表示该状态机当前激活的状态
    FsmEnemy fsmEnemy;//敌人对象，表示该有限状态机隶属于哪一个FsmEnemy（FsmEnemy表示敌人对象）
    #endregion

    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="startFsm">构造FsmEnemy时，需要传递一个StartFsm脚本对象过来</param>
    public Fsm(FsmEnemy fsmEnemy)
    {
        this.fsmEnemy = fsmEnemy;
    }
    #endregion

    #region 设置敌人状态方法
    /// <summary>
    /// 设置敌人状态方法
    /// </summary>
    /// <param name="newState">新状态</param>
    public void SetEnemyState(EnemyBaseState newState)
    {
        state = newState;
    }
    #endregion

    #region 获取敌人基本状态对象或状态名称
    /// <summary>
    /// 获取基本状态对象
    /// </summary>
    /// <returns></returns>
    public EnemyBaseState GetEnemyState()
    {
        return state;
    }

    /// <summary>
    /// 获取基本状态对象的类型名称
    /// </summary>
    /// <returns></returns>
    public string GetEnemyStateName()
    {
        return state.GetType().Name;
    }
    #endregion


    #region 获取开启有限状态机的脚本对象
    /// <summary>
    /// 获取开启有限状态机的脚本对象
    /// </summary>
    /// <returns>返回StartFsm对象</returns>
    public FsmEnemy GetFsmEnemy()
    {
        return fsmEnemy;
    }
    #endregion

    #region Handle
    /// <summary>
    /// 当前状态行为方法
    /// </summary>
    public void Handle()
    {

    }
    #endregion

    #region 更新:此Update方法在FsmEnemy的Update方法中调用，即此处的Update方法相当于Unity的Update方法
    /// <summary>
    /// 调用当前状态的行为方法
    /// 此Update方法在FsmEnemy的Update方法中调用，即此处的Update方法相当于Unity的Update方法
    /// </summary>
    public void Update()
    {
        state.Handle();
        state.Update();
    }
    #endregion
}