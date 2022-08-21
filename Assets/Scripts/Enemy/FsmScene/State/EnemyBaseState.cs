using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人状态接口：定义更新方法与状态行为的接口方法
/// </summary>
public interface EnemyBaseState
{
    void Handle();
    void Update();
}
