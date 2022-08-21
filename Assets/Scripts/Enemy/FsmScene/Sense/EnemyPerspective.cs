using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视见感知类：模拟人类视力感知系统
/// </summary>
public class EnemyPerspective : Sense
{
    public static EnemyPerspective instance;

    #region 视力感知相关参数
    int FieldOfView = 60;//视度大小
    int ViewDistance = 20;//可见距离
    int enemyLayerMask;//过滤射线碰撞的层级
    #endregion

    #region 环境中存在的物体
    GameObject player;
    GameObject[] guns;
    GameObject[] potions;
    GameObject[] boxs;
    GameObject[] walls;
    #endregion

    #region 其它变量引用
    Vector3 enemyToPlayerDir;    //敌人与玩家之间向量方向
    Vector3 enemyToGunDir;      //敌人与枪械之间向量方向
    Vector3 enemyToPotionDir;    //敌人与药瓶之间向量方向
    float distance; //玩家与场景中物品的距离
    Player playerScript;//玩家的Player.cs脚本对象
    Transform playerTrans;//玩家的Transform组件
    #endregion
    Dictionary<float, GameObject> distanceFromEnemyDic = new Dictionary<float, GameObject>();//距离敌人位置字典，GameObject表示距离敌人的游戏对象，float表示与敌人的距离

    #region 标识敌人状态的变量
    public bool isInPickState = false;
    public bool isInRunAwayState = false;
    public bool isInHideState = false;
    public bool isInEscapeState = false;
    #endregion

    #region 玩家自身属性
    int hpValue;
    int magicValue;
    int currentHPValue;
    int currentMagicValue;
    #endregion


    #region 初始化
    protected override void Initialize()
    {
        instance = this;
        //获取游戏场景中枪械，玩家和药瓶对象
        guns = ItemManager.instance.guns;
        player = ItemManager.instance.player;
        potions = ItemManager.instance.potions;
        

        //获取敌人对象的fsmEnemy脚本与有限状态机对象
        fsmEnemy = transform.GetComponent<FsmEnemy>();
        fsm = fsmEnemy.fsm;

        enemyLayerMask = LayerMask.NameToLayer("Enemy");//要过滤的射线检测层级

        //获取玩家Player脚本对象与transform
        playerScript = player.GetComponent<Player>();
        playerTrans = player.transform;
        

        hpValue = fsmEnemy.hpValue;
        magicValue = fsmEnemy.magicValue;
        
    }
    #endregion







    #region 更新
    protected override void UpdateSense()
    {
        elapsedTime += Time.deltaTime;
        
        ///状态优先级：死亡状态>逃跑状态>隐藏状态>拾取状态>逃亡状态>攻击状态>巡逻状态
        ///在FsmEnmey的碰撞检测中判断自身血量，如果血量为0处于死亡状态，直接销毁玩家对象，不用对死亡状态进行判断；如果血量小于百分之十，进入逃跑状态
        ///如果血量小于百分之五，进入逃跑状态，逃跑状态不会进行其它操作，直到进入草丛进入隐藏状态，隐藏状态缓慢回血到百分之五十后再切换成巡逻状态
        ///为什么不在敌人感知系统中判断玩家血量，原因是敌人感知系统1秒才检测一次
        if(elapsedTime >= detectionRate)
        {
            if (isInRunAwayState)//判断是否处于逃跑状态，处于逃跑状态时不切换其它状态，除非进入草丛切换到隐藏状态
            {
                elapsedTime = 0;
            }
            else if (isInHideState)//如果不处于逃跑状态，判断是否处于隐藏状态，处于隐藏状态时判断自身血量是否大于百分之五十，是则切换巡逻状态
            {
                DetectSelf();
                elapsedTime = 0;
            }
            else if (isInPickState)//不处于逃跑状态和隐藏状态，判断是否处于拾取状态，处于拾取状态过程不做其它操作
            {
                elapsedTime = 0;
            }
            else
            {
                DetectPlayer();//检测玩家，可能进入逃亡状态和攻击状态
                if (isInEscapeState)
                {
                    elapsedTime = 0;
                }
                else//如果不处于逃亡状态,检测枪械
                {
                    DetectGuns();
                    if (isInPickState)//如果检测到枪械，会进入拾取状态
                    {
                        elapsedTime = 0;
                    }
                    else
                    {
                        DetectPotions();//如果检测不到枪械，则检测药瓶
                        elapsedTime = 0;
                    }
                }
            } 
        }
    }
    #endregion





    

    #region 检测视锥范围内物品的方法
    /// <summary>
    /// 视力检测玩家方法
    /// </summary>
    private void DetectPlayer()
    {
        enemyToPlayerDir = player.transform.position - transform.position;//敌人到玩家的向量
        if (GetAngleWithItem(enemyToPlayerDir) < FieldOfView && GetDistanceWithItem(playerTrans) < ViewDistance)//如果玩家在视度范围内与视力范围内，进行射线检测
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, enemyToPlayerDir, ViewDistance, ~(1 << enemyLayerMask));//~(1 << playerLayerMask):表示不检测layermask这一层
            if (hit.collider)//如果检测到物品
            {
                string currentState = fsm.GetEnemyStateName();//获取当前状态名称
                Aspect aspect = hit.collider.GetComponent<Aspect>();//获取检测到的物体的aspect属性
                if (aspect != null && aspect.aspectName == playerAspect)//如果检测到了玩家
                {
                    //获取玩家当前手持武器等级与敌人自身等级，如果两者相差超过3，则敌人进入逃跑状态   如果玩家处于狂暴状态，则敌人进入逃跑状态
                    Debug.Log("检测到玩家");
                    int enemyLevel = fsmEnemy.level;
                    bool playerIsRage = playerScript.isRage;

                    int playerGunLever = playerScript.currentHandle.GetComponent<Gun>().level;//获取武器等级
                    if (currentState == FsmEnemy.FsmEnemyStateEnum.PatrolState.ToString())//如果敌人处于巡逻状态
                    {

                        if (playerGunLever - enemyLevel >= 3 || playerIsRage == true)
                        {
                            Debug.Log("玩家武器等级过高或处于狂暴状态");
                            fsm.SetEnemyState(new EscapeState(fsm));//改变状态为逃跑状态
                            isInEscapeState = true;
                        }
                        else
                        {
                            fsm.SetEnemyState(new AttackState(fsm));//改变状态为攻击状态
                        }
                    }
                    else if (currentState == FsmEnemy.FsmEnemyStateEnum.AttackState.ToString())//如果敌人处于攻击状态
                    {
                        if (playerGunLever - enemyLevel >= 3 || playerIsRage == true)
                        {
                            Debug.Log("玩家武器等级过高或处于狂暴状态");
                            fsm.SetEnemyState(new EscapeState(fsm));//改变状态为逃跑状态
                            isInEscapeState = true;
                        }
                    }
       
                }
                else//代码走这里说明识别不到玩家
                {
                    if (currentState == FsmEnemy.FsmEnemyStateEnum.AttackState.ToString())//如果敌人处于攻击状态
                    {
                        fsm.SetEnemyState(new PatrolState(fsm));
                    }
                }
            }
        }
    }







    /// <summary>
    /// 视力检测枪械方法
    /// </summary>
    private void DetectGuns()
    {
        InstantiateAndOrderDic(guns);//初始化字典
        List<float> keys = GetDicKeys(distanceFromEnemyDic);//获取字典的所有key
        keys.Sort();//对key的集合进行排序

        for (int i = 0; i < keys.Count; i++)
        {
            GameObject gun;
            distanceFromEnemyDic.TryGetValue(keys[i],out gun);//根据key取Value
            Transform gunTrans = gun.transform;//获取gun的组件
            enemyToGunDir = gunTrans.position - transform.position;//获取gun到敌人的方向向量
            if (GetAngleWithItem(enemyToGunDir) < FieldOfView && GetDistanceWithItem(gunTrans) < ViewDistance)//如果枪械在视度范围内与视力范围内，进行射线检测
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, enemyToGunDir, ViewDistance, ~(1 << enemyLayerMask));//~(1 << playerLayerMask):表示不检测layermask这一层
                if (hit.collider)//如果检测到物品
                {
                    string currentState = fsm.GetEnemyStateName();//获取当前状态名称
                    Aspect aspect = hit.collider.GetComponent<Aspect>();//获取检测到的物体的aspect属性

                    if (aspect != null && aspect.aspectName == gunAspect)
                    {
                        //如果敌人不处于逃跑状态，且枪械的等级大于敌人自身等级3，则进入拾取状态
                        //如果处于逃跑状态且玩家武器大于自身等级3，且拾取枪械后小于3，则进入拾取状态
                        Debug.Log("检测到枪械");

                        int enemyLevel = fsmEnemy.level;//获取敌人等级
                        bool playerIsRage = playerScript.isRage;//获取玩家是否处于狂暴状态
                        int gunLevel = gunTrans.GetComponent<Gun>().level;//获取检测到的武器的武器等级
                        int playerGunLevel = playerScript.currentHandle.GetComponent<Gun>().level;//获取玩家手持武器等级

                        if (fsmEnemy.level < 5 && currentState != FsmEnemy.FsmEnemyStateEnum.EscapeState.ToString() && gunLevel >= enemyLevel)//如果敌人不处于逃跑状态且武器等级大于自身等级
                        {
                            fsm.SetEnemyState(new PickState(fsm, gun));//进入拾取状态
                            isInPickState = true;
                            return;
                        }
                        else if (fsmEnemy.level < 5 && playerIsRage == false && currentState == FsmEnemy.FsmEnemyStateEnum.AttackState.ToString() && playerGunLevel - enemyLevel == 3)//如果敌人因为玩家武器等级过高而处于逃跑状态，且敌人拾取武器后自身等级加1小于3，则进入拾取状态
                        {
                            fsm.SetEnemyState(new PickState(fsm, gun));//进入拾取状态
                            isInPickState = true;
                            return;
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// 视力检测药瓶方法
    /// </summary>
    private void DetectPotions()
    {
        InstantiateAndOrderDic(potions);//初始化字典
        List<float> keys = GetDicKeys(distanceFromEnemyDic);//获取字典的所有key
        keys.Sort();//对key的集合进行排序

        for (int i = 0; i < keys.Count; i++)
        {
            GameObject potion;
            distanceFromEnemyDic.TryGetValue(keys[i], out potion);//根据key取Value
            Transform potionTrans = potion.transform;//获取gun的组件
            enemyToPotionDir = potionTrans.position - transform.position;
            if (GetDistanceWithItem(potionTrans) < ViewDistance && GetAngleWithItem(enemyToPotionDir) < FieldOfView)//如果枪械在视度范围内与视力范围内，进行射线检测
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, enemyToPotionDir, ViewDistance, ~(1 << enemyLayerMask));//~(1 << playerLayerMask):表示不检测layermask这一层
                if (hit.collider)//如果检测到物品
                {
                    string currentState = fsm.GetEnemyStateName();//获取当前状态名称
                    Aspect aspect = hit.collider.GetComponent<Aspect>();//获取检测到的物体的aspect属性

                    if (aspect != null && aspect.aspectName == potionAspect)
                    {

                        Debug.Log("检测到药瓶");

                        fsm.SetEnemyState(new PickState(fsm, potion));
                        isInPickState = true;
                        return;
                    }
                }
            }
        }


        /*for (int i = 0; i < potions.Length; i++)
        {
            Transform potionTrans = potions[i].transform;
            enemyToPotionDir = potionTrans.position - transform.position;
            if (GetDistanceWithItem(potionTrans) < ViewDistance  && GetAngleWithItem(enemyToPotionDir) < FieldOfView)//如果枪械在视度范围内与视力范围内，进行射线检测
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, enemyToPotionDir, ViewDistance, ~(1 << enemyLayerMask));//~(1 << playerLayerMask):表示不检测layermask这一层
                if (hit.collider)//如果检测到物品
                {
                    string currentState = fsm.GetEnemyStateName();//获取当前状态名称
                    Aspect aspect = hit.collider.GetComponent<Aspect>();//获取检测到的物体的aspect属性

                    if (aspect != null && aspect.aspectName == potionAspect)
                    {
                        
                        Debug.Log("检测到药瓶");

                        fsm.SetEnemyState(new PickState(fsm, potions[i]));
                        
                    }
                }
            }
        }*/
    }
    
    /// <summary>
    /// 检测玩家自身，当敌人血量大于百分之五十时，进入巡逻状态
    /// </summary>
    private void DetectSelf()
    {
        currentHPValue = fsmEnemy.currentHPValue;
        currentMagicValue = fsmEnemy.currentMagicValue;
        if ((float)Math.Round((decimal)currentHPValue / hpValue, 2) >= 0.5f)
        {
            Debug.Log("检查自身");
            Debug.Log("当前血量" + currentHPValue.ToString());
            Debug.Log("血量" + hpValue.ToString());
            Debug.Log((float)Math.Round((decimal)currentHPValue / hpValue, 2));
            fsm.SetEnemyState(new PatrolState(fsm));
            isInHideState = false;
        }
    }
    #endregion








    #region 获取敌人与物体之间的距离和角度
    /// <summary>
    /// 获取与物体的距离
    /// </summary>
    /// <param name="gameObject"></param>
    private float GetAngleWithItem(Vector3 dir)
    {
        float angle = 0;
        if (transform.rotation.y == 180)//如果怪物朝右看
        {
            angle = Vector3.Angle(-transform.right, dir);//敌人自身向前向量与敌人和玩家向量的夹角
        }
        else if (transform.rotation.y == 0)//如果怪物朝坐看
        {
            angle = Vector3.Angle(-transform.right, dir);
        }

        return angle;
    }



    /// <summary>
    /// 获取与物体向量的距离
    /// </summary>
    /// <param name="gameObject"></param>
    private float GetDistanceWithItem(Transform trans)
    {
        return Mathf.Abs(Vector3.Distance(trans.position, transform.position));//敌人到玩家的向量距离
    }
    #endregion









    #region 画线方法：绘制敌人视力范围
    /// <summary>
    /// OnDrawGizmos：随程序启动就运行且每帧运行一次
    /// 用于绘制敌人与GameObject对象之间的视力范围
    /// </summary>
    /*private void OnDrawGizmos()
    {
        //绘制敌人到玩家的直线
        Debug.DrawLine(transform.position, player.transform.position, Color.red);


        if (transform.rotation.y == 180)//如果怪物朝右看
        {
            Vector3 frontRayPoint = transform.position + (-transform.right * ViewDistance);
            Vector3 leftRayPotin = frontRayPoint;
            leftRayPotin.y += FieldOfView;
            Vector3 rightRayPoint = frontRayPoint;
            rightRayPoint.y -= FieldOfView;


            Debug.DrawLine(transform.position, frontRayPoint, Color.green);
            Debug.DrawLine(transform.position, leftRayPotin, Color.green);
            Debug.DrawLine(transform.position, rightRayPoint, Color.green);
        }
        else if (transform.rotation.y == 0)//如果怪物朝坐看
        {
            Vector3 frontRayPoint = transform.position + (-transform.right * ViewDistance);
            Vector3 leftRayPotin = frontRayPoint;
            leftRayPotin.y += FieldOfView;
            Vector3 rightRayPoint = frontRayPoint;
            rightRayPoint.y -= FieldOfView;


            Debug.DrawLine(transform.position, frontRayPoint, Color.green);
            Debug.DrawLine(transform.position, leftRayPotin, Color.green);
            Debug.DrawLine(transform.position, rightRayPoint, Color.green);
        }
    }*/
    #endregion









   

    /// <summary>
    /// 获取游戏对象与玩家距离
    /// </summary>
    /// <returns></returns>
    private float GetDistance(GameObject Object)
    {
        return Vector3.Distance(Object.transform.position, transform.position);
    }

    /// <summary>
    /// 初始化字典
    /// </summary>
    /// <param name="gameObjects"></param>
    private void InstantiateAndOrderDic(GameObject[] gameObjects)
    {
        distanceFromEnemyDic.Clear();//先清空字典
        
        for(int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i])
            {
                distanceFromEnemyDic.Add(GetDistance(gameObjects[i]), gameObjects[i]);
            }
            
        }
    }

    private List<float> GetDicKeys(Dictionary<float,GameObject> dic)
    {
        List<float> keys = new List<float>();
        foreach(KeyValuePair<float,GameObject> kvp in dic)
        {
            keys.Add(kvp.Key);
        }
        return keys;
    }
}
