using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 子弹与箱子碰撞，销毁子弹和箱子，概率掉落金币、武器、药水
/// 获取箱子的瓦片地图，并在Start方法内初始化
/// </summary>
public class Boxes : MonoBehaviour
{
    #region 武器或药水预制体引用集合
    public List<GameObject> guns = new List<GameObject>();
    public List<GameObject> potions = new List<GameObject>();
    #endregion

    #region 引用
    Tilemap boxTilemap;     
    //GameObject[] tileMapGameObject;//如果箱子类型有很多，需要用一个数组来存放这些箱子，在子弹boxTilemap物体碰撞时，判断碰撞的物体是不是包含在这里面，如果是说明是箱子
    #endregion

    private void Start()
    {
        boxTilemap = GameObject.Find("Grid").transform.Find("Boxes").GetComponent<Tilemap>();
        
    }

    /// <summary>
    /// 初始化箱子
    /// </summary>
    private void InitializeBoxes()
    {

    }

    /// <summary>
    /// 如果子弹和Boxes(Tilemap)的瓦片发生碰撞，消除该位置瓦片
    /// 定义一个三维变量用于存放碰撞位置
    /// 判断与boxTilemap发生碰撞的物体是不是子弹，方法是定义子弹的层级，如果层级对应的是子弹，则获取子弹碰撞点collision.contacts
    /// 将子弹碰撞点的x轴和y轴保存到hitPosition中
    /// 调用boxTilemap的WorldToCell方法将hitPosition坐标转换为地图网格坐标，并调用Tilemap的SetTile方法将改坐标的瓦片设置为null（销毁箱子）
    /// 
    /// 概率掉落武器或药水
    /// </summary>
    /// <param name="collision">碰撞的物体</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector3 hitPosition = Vector3.zero; //碰撞点向量
        if(boxTilemap != null)
        {
            if (collision.gameObject.layer == 15 || collision.gameObject.layer == 14)
            {
                foreach (ContactPoint2D hit in collision.contacts)//遍历碰撞接触点
                {
                    hitPosition.x = hit.point.x - 0.01f * hit.normal.x;
                    hitPosition.y = hit.point.y - 0.01f * hit.normal.y;
                    boxTilemap.SetTile(boxTilemap.WorldToCell(hitPosition), null);//将碰撞点位置转换到网格坐标位置，并将其置为空
                }
                Destroy(collision.gameObject);  //销毁子弹对象

                int num1 = Random.Range(1, 101);//随机生成一个0到100的数
                if(num1 > 90)
                {
                    int num2 = Random.Range(1, 100);//再随机生成一个0到100的数
                    if(num2 < 50)
                    {
                        Instantiate(guns[Random.Range(0, guns.Count - 1)], boxTilemap.WorldToCell(hitPosition), this.transform.rotation);//随机生成一把武器
                    }
                    else
                    {
                        Instantiate(potions[Random.Range(0, potions.Count - 1)], boxTilemap.WorldToCell(hitPosition), this.transform.rotation);//随机生成药水
                    }
                }
            }
            
        }
    }
}
