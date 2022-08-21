using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 拾取状态
/// </summary>
public class PickState : EnemyBaseState
{
    #region 变量与引用
    #region 有限状态机对象与开启有限状态机的脚本对象
    private Fsm fsm;//获取敌人有限状态机对象
    private FsmEnemy fsmEnemy;//获取敌人对象脚本对象
    #endregion

    #region 可拾取物品的相关变量与引用
    GameObject pickItemObject;//需要去拾取的物品
    Vector3 dir;//前往拾取的方向
    GameObject[] fsmEnemyPrefabs;
    #endregion

    #region Npc相关变量与引用
    Rigidbody2D rig;//敌人刚体对象
    List<GameObject> pickableItems;//敌人可拾取游戏对象引用
    int hpValue;
    int magicValue;
    int currentHPValue;
    int currentMagicValue;
    int damage;
    float moveSpeed;//敌人的移动速度
    int level;//敌人等级
    bool isRage;//敌人是否处于狂暴状态
    float shootSpeed;//敌人射击速度
    #endregion
    #endregion
    #region 构造方法
    public PickState(Fsm fsm, GameObject pickItemObject)
    {
        Debug.Log("进入拾取状态");
        this.fsm = fsm;
        this.fsmEnemy = fsm.GetFsmEnemy();
        this.pickItemObject = pickItemObject;
        this.pickableItems = fsmEnemy.pickableItems;
        this.rig = fsmEnemy.rig;
        this.fsmEnemyPrefabs = fsmEnemy.fsmEnemyPrefabs;
    }
    #endregion
    #region 更新
    //Handle和Update方法都是在StartFsm的Update方法中调用，即Handle和Update方法都相当于Unity的Update方法，每帧调用一次
    public void Handle()
    {
        if(pickItemObject.tag == "EnvironmentWeapon")
        {
            Debug.Log("开始拾取枪械");
            PickGun();
        }
        else if(pickItemObject.tag == "Potion")
        {
            Debug.Log("开始拾取药瓶");
            PickPotion();
        }
    }
    public void Update()
    {

    }
    #endregion


    private void PickGun()
    {
        this.moveSpeed = fsmEnemy.moveSpeed;
        this.fsmEnemyPrefabs = fsmEnemy.fsmEnemyPrefabs;
        this.level = fsmEnemy.level;
        Vector3 dir = pickItemObject.transform.position - fsmEnemy.transform.position;
        rig.velocity = dir * Time.deltaTime * moveSpeed * 100;
        if (pickableItems.Contains(pickItemObject))//如果可拾取物品在列表内
        {
            GameObject.Instantiate(fsmEnemyPrefabs[level + 1], fsmEnemy.transform.position, fsmEnemy.transform.rotation);//等级+1
            fsmEnemy.DestroyItem(pickItemObject);//删除拾取对象，表示拾取成功
            fsmEnemy.Dead();//原对象销毁
        }
    }
    private void PickPotion()
    {
        this.hpValue = fsmEnemy.hpValue;
        this.magicValue = fsmEnemy.magicValue;
        this.currentHPValue = fsmEnemy.currentHPValue;
        this.currentMagicValue = fsmEnemy.currentMagicValue;
        this.damage = fsmEnemy.damage;
        this.shootSpeed = fsmEnemy.shootSpeed;
        Vector3 dir = pickItemObject.transform.position - fsmEnemy.transform.position;
        rig.velocity = dir * Time.deltaTime * moveSpeed * 100;
        if (pickableItems.Contains(pickItemObject))//如果可拾取物品在可拾取集合内
        {
            switch (pickItemObject.GetComponent<Potion>().potionName)
            {
                case "HealthyPotion":
                    this.currentHPValue += 50;
                    if (this.currentHPValue > hpValue)
                    {
                        this.currentHPValue = hpValue;
                    }
                    fsmEnemy.DestroyItem(pickItemObject);
                    break;
                case "MagicPotion":
                    this.currentMagicValue += 50;
                    if (this.currentMagicValue > magicValue)
                    {
                        this.currentMagicValue = magicValue;
                    }
                    fsmEnemy.DestroyItem(pickItemObject);
                    break;
                case "RejuvenationPotion":
                    this.currentHPValue += 50;
                    if (this.currentHPValue > hpValue)
                    {
                        this.currentHPValue = hpValue;
                    }
                    this.currentMagicValue += 50;
                    if (this.currentMagicValue > magicValue)
                    {
                        this.currentMagicValue = magicValue;
                    }
                    fsmEnemy.DestroyItem(pickItemObject);
                    break;
                case "AgilityPotion":
                    this.moveSpeed += 1;
                    fsmEnemy.DestroyItem(pickItemObject);
                    break;
                case "RagePotion":
                    isRage = true;
                    this.moveSpeed += 5;
                    this.shootSpeed -= 0.1f;
                    this.damage += 10;
                    fsmEnemy.StartCoroutineInState("Calm"); 
                    fsmEnemy.DestroyItem(pickItemObject);

                    break;
                default:
                    break;
            }
        }
        fsm.SetEnemyState(new PatrolState(fsm));//进入巡逻状态
        EnemyPerspective.instance.isInPickState = false;
    }
}
