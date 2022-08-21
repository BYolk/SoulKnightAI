using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品管理器脚本，在此脚本中存放场景中所有物品
/// </summary>
public class ItemManager : MonoBehaviour
{
    #region 单例
    public static ItemManager instance;
    #endregion

    #region 场景中物品相关变量
    public GameObject player;
    public GameObject[] guns;
    public GameObject[] potions;
    public GameObject[] boxes;
    public GameObject[] walls;
    public GameObject[] obstacles;
    public GameObject bush;
    #endregion
    #region 检测场景中物品相关参数
    public float updateRate;//更新速率
    private float elapsedTime;//经过的时间
    #endregion
    #region 初始化
    private void Awake()
    {
        instance = this;
        updateRate = 1.0f;//初始化更新速率为1s，每过1s更新物品数组
        GetItemsInScene();
    }
    #endregion
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= updateRate)
        {
            GetItemsInScene();
        }
    }
    private void GetItemsInScene()
    {
        elapsedTime = 0;
        FindGameObjects();
    }

    private void FindGameObjects()
    {
        player = GameObject.Find("Player");
        guns = GameObject.FindGameObjectsWithTag("EnvironmentWeapon");
        potions = GameObject.FindGameObjectsWithTag("Potion");
        boxes = GameObject.FindGameObjectsWithTag("Boxes");
        walls = GameObject.FindGameObjectsWithTag("Wall");
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        bush = GameObject.FindGameObjectWithTag("Bush");
    }



}
