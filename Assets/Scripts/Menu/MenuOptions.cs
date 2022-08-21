using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// 菜单场景中，点击按钮跳转到相应的游戏场景
/// </summary>
public class MenuOptions : MonoBehaviour
{
    #region

    #endregion
    
    /// <summary>
    /// 跳转到“标准模式场景”
    /// </summary>
    public void StrandardScene()
    {
        SceneManager.LoadScene(1);
    }
    /// <summary>
    /// 跳转到FSM场景
    /// </summary>
    public void FSMScene()
    {
        SceneManager.LoadScene(2);
    }
    /// <summary>
    /// 跳转到FuSM场景
    /// </summary>
    public void FuSMScene()
    {
        SceneManager.LoadScene(3);
    }
    /// <summary>
    /// 跳转到ANN场景
    /// </summary>
    public void ANNScene()
    {
        SceneManager.LoadScene(4);
    }
    public void Exit()
    {
        Application.Quit();
    }
}
