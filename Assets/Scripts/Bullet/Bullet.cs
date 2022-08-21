using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region

    #endregion
    public static Bullet instance;

    #region 变量
    public LayerMask bulletLayer;//子弹层级
    public float speed = 2000;//子弹速度
    public int damage = 5;
    #endregion

    #region 引用
    #endregion

    #region 初始化
    private void Awake()
    {
        instance = this;
    }
    #endregion
}
