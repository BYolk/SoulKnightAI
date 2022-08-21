using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 如果子弹与障碍物发生碰撞，销毁子弹
/// </summary>
public class Obstacle : MonoBehaviour
{
    #region

    #endregion


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "PlayerBullet" || collision.gameObject.tag == "EnemyBullet")//14和15层级分别对应玩家子弹和敌人子弹
        {
            Destroy(collision.gameObject);
        }
    }
}
