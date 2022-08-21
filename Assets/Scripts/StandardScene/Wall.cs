using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 如果墙与子弹发生碰撞，销毁子弹
/// </summary>
public class Wall : MonoBehaviour
{
    #region

    #endregion


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 15 || collision.gameObject.layer == 14)//14和15层级分别对应玩家子弹和敌人子弹
        {
            Destroy(collision.gameObject);
        }
    }
}
