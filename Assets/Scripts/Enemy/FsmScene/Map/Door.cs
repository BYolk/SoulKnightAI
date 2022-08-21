using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 传送门脚本：用于敌人溃逃时用，当敌人进入传送门，表示敌人逃跑成功
/// </summary>
public class Door : MonoBehaviour
{
    #region

    #endregion

    /// <summary>
    /// 传送门的碰撞检测方法：如果与传送门碰撞的时敌人类型且敌人处于逃跑状态
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        
        /*if(tag == "Enemy" )
        {
            if(collision.GetComponent<FsmEnemy>().fsm.GetEnemyState().GetType().Name == "EscapeState")
            {
                Debug.Log(collision.gameObject.name.ToString() + "逃跑成功");
                Destroy(collision.gameObject);
            }
        }*/
        if(tag == "PlayerBullet" || tag == "EnemyBullet")
        {
            Destroy(collision.gameObject);
        }
    }


    
}
