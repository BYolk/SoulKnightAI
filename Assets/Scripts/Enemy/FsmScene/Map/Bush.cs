using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Enemy")//如果敌人进入草丛
        {
            FsmEnemy fsmEnemy = collision.gameObject.GetComponent<FsmEnemy>();
            Fsm fsm = fsmEnemy.fsm;
            EnemyPerspective enemyPerspective = collision.gameObject.GetComponent<EnemyPerspective>();

            if(fsm.GetEnemyStateName() == "RunAwayState")//如果敌人处于逃跑状态
            {
                enemyPerspective.isInRunAwayState = false;
                enemyPerspective.isInHideState = true;
                fsm.SetEnemyState(new HideState(fsm));//切换敌人到隐藏状态
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            FsmEnemy fsmEnemy = collision.gameObject.GetComponent<FsmEnemy>();
            Fsm fsm = fsmEnemy.fsm;
            EnemyPerspective enemyPerspective = collision.gameObject.GetComponent<EnemyPerspective>();

            int currentHPValue = fsmEnemy.currentHPValue;
            int hpValue = fsmEnemy.hpValue;
            if (fsm.GetEnemyStateName() == "HideState")//如果敌人处于隐藏状态
            {
                if((float)Math.Round((decimal)currentHPValue / hpValue, 2) >= 0.5f)//如果血量大于百分之五十，切换为巡逻状态
                {
                    fsm.SetEnemyState(new PatrolState(fsm));
                    enemyPerspective.isInHideState = false;
                }
            }
        }
    }
}
