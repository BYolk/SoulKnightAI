using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 让自定义的感知系统继承该类
/// </summary>
public class Sense : MonoBehaviour
{
    //定义每种游戏对象的Aspect
    public Aspect.aspect playerAspect = Aspect.aspect.PLAYER;
    public Aspect.aspect gunAspect = Aspect.aspect.GUN;
    public Aspect.aspect potionAspect = Aspect.aspect.POTION;
    public Aspect.aspect wallAspect = Aspect.aspect.WALL;
    public Aspect.aspect obstacleAspect = Aspect.aspect.OBSTACLE;
    public Aspect.aspect bushAspect = Aspect.aspect.BUSH;
    public Aspect.aspect boxAspect = Aspect.aspect.BOX;
    
    protected FsmEnemy fsmEnemy;
    protected Fsm fsm;
    public float detectionRate;//感知检测率
    protected float elapsedTime;//计时
    protected virtual void Initialize() { }
    protected virtual void UpdateSense() { }
    private void Start()
    {
        elapsedTime = 0.0f;
        detectionRate = ItemManager.instance.updateRate;
        Initialize();
    }
    private void Update()
    {
        UpdateSense();
    }
}
