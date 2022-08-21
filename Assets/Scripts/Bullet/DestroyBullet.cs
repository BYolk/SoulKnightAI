using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 销毁（回收）对象脚本
/// </summary>
public class DestroyBullet : MonoBehaviour
{

    #region 变量
    float activeTime = 3;//子弹存活时间
    #endregion

    #region 引用
    GameObject bullet;//要回收的游戏对象
    Rigidbody2D rig;//子弹对象的刚体:用于设置子弹速度
    #endregion

    #region 初始化
    private void Start()
    {
        bullet = this.gameObject;
        rig = this.GetComponent<Rigidbody2D>();
    }
    #endregion

    #region 更新
    private void Update()
    {
        activeTime -= Time.deltaTime;
        if(activeTime < 0)
        {
            if (this.gameObject.CompareTag("PlayerBullet"))
            {
                PlayerBulletPoolManager.instance.recoverBullet(bullet);
            }
            if (this.gameObject.CompareTag("EnemyBullet"))
            {
                EnemyBulletPoolManager.instance.recoverBullet(bullet);
            }
        }
    }
    #endregion

    /// <summary>
    /// 回收子弹对象后，该子弹对象就被禁用（速度为0）
    /// </summary>
    private void OnDisable()
    {
        rig.velocity = Vector3.zero;
    }

    /// <summary>
    /// 激活子弹对象：从子弹对象池中取出子弹对象，激活该子弹对象,为子弹对象赋值，添加刚体
    /// </summary>
    private void OnEnable()
    {
        if(rig == null)
        {
            bullet = this.gameObject;
            rig = GetComponent<Rigidbody2D>();
        }
        activeTime = 3;
    }
}
