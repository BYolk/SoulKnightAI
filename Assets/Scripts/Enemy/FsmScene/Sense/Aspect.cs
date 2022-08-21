using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 感知系统的Aspect类
/// </summary>
public class Aspect : MonoBehaviour
{
    /// <summary>
    /// 感知系统需要感知的游戏对象
    /// </summary>
    public enum aspect
    {
        PLAYER,
        GUN,
        POTION,
        BOX,
        WALL,
        OBSTACLE,
        BUSH
    }

    public aspect aspectName;
}
