using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 模糊状态机基本状态类
/// </summary>
public interface FusmBaseState
{
    float Evaluate();//计算是否足以激活模糊状态
    void Update();//更新
    //检查边界，确保Evaluate返回的激活因子的值的合法性
    void CheckLowerBound(float lowerBound = 0.0f);//检测最低边界
    void CheckUpperBound(float upperBound = 1.0f);//检测最高边界
    void CheckBounds(float lowerBound = 0.0f, float upperBound = 1.0f);//检测边界








    void Exit();
    void Init();//初始化



    /*public float activation;//判断切换模糊状态机的变量
    protected FusmEnemy fusmEnemy;//敌人对象

    public virtual void Enter() { }//进入状态执行方法
    public virtual void Exit() { }//退出状态执行方法
    public abstract void UpdateFusmBaseState();//更新
    public virtual void Init() { activation = 0.0f; }//初始化
    public abstract float CalculateActivation();//计算启动因素，是否足以切换模糊状态机

    public virtual void CheckLowerBound(float lbound = 0.0f) { if (activation < lbound) activation = lbound; }//检测最低边界
    public virtual void CheckUpperBound(float ubound = 1.0f) { if (activation > ubound) activation = ubound; }//检测最高边界
    public virtual void CheckBounds(float lb = 0.0f, float ub = 1.0f) { CheckLowerBound(lb); CheckUpperBound(ub); }//检测边界*/

}
