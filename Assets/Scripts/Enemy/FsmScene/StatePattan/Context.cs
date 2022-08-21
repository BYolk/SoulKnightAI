using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 上下文对象，是状态模式中的状态管理器
///     1、定义State接口属性，表示当前上下文（环境）中的当前状态
///     2、定义构造方法
/// </summary>
public class Context
{
    private EnemyBaseState enemyBaseState;

    //Context构造方法
    public Context(EnemyBaseState enemyBaseState)
    {
        this.EnemyState = enemyBaseState;
    }

    //State属性
    public EnemyBaseState EnemyState
    {
        get { return enemyBaseState; }
        set
        {
            enemyBaseState = value;
            Debug.Log("State: " + enemyBaseState.GetType().Name);
        }
    }

    /// <summary>
    /// 调用当前状态对应的方法
    /// </summary>
    public void Request()
    {
        enemyBaseState.Handle();
    }
}
