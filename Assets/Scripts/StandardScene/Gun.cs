using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    #region

    #endregion


    #region 变量
    public string weaponName;
    public int level = 0;                   //武器等级，0为最低，5为最高
    public float cooling = 0.02f;           //武器冷却时间，值越小，射击速度越快
    public int damage = 5;                  //武器伤害
    public int consumeMagic = 0;
    public Vector3 weaponPos;               //武器位置
    #endregion






    #region 引用
    public static Gun instance;
    Button pickButton;
    BoxCollider2D weaponCollider;
    #endregion







    #region 初始化
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        weaponCollider = this.gameObject.GetComponent<BoxCollider2D>();
        if (weaponCollider == null)
        {
            weaponCollider = this.gameObject.AddComponent<BoxCollider2D>();
            weaponCollider.isTrigger = true;
        }
    }
    #endregion





    #region 触发方法与退出触发方法
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //如果武器没有父物体，说明不是装备再身上的武器,因为玩家身上有武器，武器也带有碰撞器，也会和地上的武器发生触发，所以这里要判断只有Player身上的触发器可以参与触发
        if (this.transform.parent == null && collision.gameObject.tag == "Player") 
        {

            collision.gameObject.GetComponent<Player>().pickableItems.Add(this.gameObject);
        }
        if (this.transform.parent == null && collision.gameObject.tag == "Enemy")
        {
            if (collision.gameObject.GetComponent<Enemy>())
            {
                collision.gameObject.GetComponent<Enemy>().pickableItems.Add(this.gameObject);
            }
            else if (collision.gameObject.GetComponent<FsmEnemy>())
            {
                collision.gameObject.GetComponent<FsmEnemy>().pickableItems.Add(this.gameObject);
            }
            
        }
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (this.transform.parent == null && collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<Player>().pickableItems.Remove(this.gameObject);
        }
        if (this.transform.parent == null && collision.gameObject.tag == "Enemy")
        {
            if (collision.gameObject.GetComponent<Enemy>())
            {
                collision.gameObject.GetComponent<Enemy>().pickableItems.Remove(this.gameObject);
            }
            else if (collision.gameObject.GetComponent<FsmEnemy>())
            {
                collision.gameObject.GetComponent<FsmEnemy>().pickableItems.Remove(this.gameObject);
            }
        }
    }
    #endregion
}
