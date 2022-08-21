using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹对象池
/// </summary>
public class PlayerBulletPoolManager : MonoBehaviour
{
    #region

    #endregion

    #region 变量
    Queue<GameObject> playerBulletPool;                   //创建存放子弹对象的队列
    int poolInitLength = 100;                        //定义子弹对象池中的子弹对象数量
    #endregion

    #region 引用
    public static PlayerBulletPoolManager instance;       //创建脚本单例
    public GameObject bulletPrefab;                 //子弹对象引用
    #endregion

    #region 初始化
    private void Awake()
    {
        instance = this;    //实例化单例
    }
    private void Start()
    {
        playerBulletPool = new Queue<GameObject>();   //创建一个队列，用于存放子弹对象
        GameObject bullet = null;               //创建一个空的子弹对象
        for(int i = 0; i < poolInitLength; i++) //填充子弹对象池
        {
            bullet = Instantiate(bulletPrefab); //实例化子弹对象
            bullet.SetActive(false);            //禁用子弹（子弹从池里拿出来使用时再激活）
            playerBulletPool.Enqueue(bullet);         //调用队列的入队方法，将子弹填充到子弹对象池内
            bullet.transform.parent = GameObject.Find("PlayerBulletPool").transform;
        }
    }
    #endregion

    #region 使用与回收子弹对象

    /// <summary>
    /// 获取子弹对象
    /// 如果从对象池获取子弹的速度大于回收的速度，那么需要重新初始化子弹对象
    /// </summary>
    /// <returns>返回一个子弹预制体对象</returns>
    public GameObject getBullet() 
    {
        if(playerBulletPool.Count > 0)                        //如果子弹对象池（子弹队列）剩余子弹数量大于0，则从子弹对象池中取出一个子弹对象返回
        {
            GameObject bullet = playerBulletPool.Dequeue();   //调用出队方法从队列中取出一个子弹对象
            bullet.SetActive(true);                     //激活子弹对象
            return bullet;                              //将子弹对象返回
        }
        else
        {
            GameObject bullet = Instantiate(bulletPrefab);//代码走这里说明子弹对象池已经没有子弹对象了，重新初始化一个子弹对象返回
            bullet.transform.parent = GameObject.Find("PlayerBulletPool").transform;
            return bullet;           
        }
    }

    /// <summary>
    /// 回收子弹对象方法
    /// </summary>
    /// <param name="bullet">需要回收的子弹对象</param>
    public void recoverBullet(GameObject bullet)
    {
        bullet.SetActive(false);                        //将子弹对象禁用
        playerBulletPool.Enqueue(bullet);                     //将子弹对象回收到队列（对象池）中               
    }
    #endregion
}
