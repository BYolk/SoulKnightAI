using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeState : EnemyBaseState
{
    #region 有限状态机对象与开启有限状态机的脚本对象
    private Fsm fsm;//获取敌人有限状态机对象
    private FsmEnemy fsmEnemy;//获取敌人对象脚本对象
    #endregion
    #region 变量
    float nearestDistance = 9999;//距离敌人最近的门和敌人之间的距离
    float moveSpeed;
    Vector3 escapeDir;
    #endregion
    #region 引用
    GameObject[] doors;
    Transform nearestDoor;
    Transform enemyTransform;
    Rigidbody2D enemyRig;
    #endregion
    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="fsmEnemy">构造逃跑状态时，需要传递一个敌人的脚本对象FsmEnemy</param>
    public EscapeState(Fsm fsm)
    {
        Debug.Log("进入逃跑状态");
        this.fsm = fsm;
        this.fsmEnemy = fsm.GetFsmEnemy();

        
        fsmEnemy.gameObject.AddComponent<ANNController>();
        moveSpeed = fsmEnemy.moveSpeed;
        enemyRig = fsmEnemy.rig;
        enemyTransform = fsmEnemy.transform;
        doors = GameObject.FindGameObjectsWithTag("Door");
    }
    #endregion
    #region 更新
    //Handle和Update方法都是在StartFsm的Update方法中调用，即Handle和Update方法都相当于Unity的Update方法，每帧调用一次
    public void Handle()
    {
        Escape();
    }
    public void Update()
    {

    }
    #endregion
    private void Escape()
    {
        foreach(GameObject door in doors)
        {
            float tempDistance = Mathf.Abs(Vector3.Distance(door.transform.position, enemyTransform.position));
            if(tempDistance < nearestDistance)
            {
                nearestDoor = door.transform;
            }
        }
        escapeDir = (nearestDoor.position - enemyTransform.position).normalized;
        enemyRig.velocity = escapeDir * Time.deltaTime * moveSpeed * 100 * 5;
    }
}
