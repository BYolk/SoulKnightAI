using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 敌人控制器脚本思路：挂载到敌人上
///     1、程序运行，执行Awake方法：记录程序启动时敌人当前位置为开始位置，记录敌人当前角度为开始角度，并初始化敌人的神经网络
///     
/// 
///     2、每隔0.02秒执行一次FixupUpdate：调用 InputSensors 获取敌人与障碍物之间的距离，并更新当前位置为敌人的最后位置（如果下一帧敌人撞墙，那么这一帧记录的位置就是这辆敌人的最后位置）。运行敌人的神经网络（输入值是敌人三个传感器与三个方向的墙的距离），获得神经网络输出值——加速度和转向值，根据加速度和转向值调用敌人移动方法 MoveCar 移动敌人，并记录敌人移动时间 timeSinceStart ，调用 CalculateFitness 方法计算神经网络的适应度。
///         2.1、InputSensors 方法获取敌人与障碍物之间的距离：敌人有三个传感器，根据射线检测分别获取敌人正前方距离墙的距离、左前方与墙的距离和右前方与墙的距离
///         2.2、network.RunNetwork 方法：在NNet类中定义的方法，用来运行神经网络
///         2.3、MoveCar 方法：运行敌人的方法，根据神经网络输出的加速度和转向值控制敌人如何加速如何转向。调用 Vector3.Lerp 方法根据加速度计算出敌人每帧移动的距离（向量），并通过transform.TransformDirection方法将向量转化到世界坐标。再根据转向值每帧旋转敌人
///         2.4、CalculateFitness 方法：记录敌人走过的总路程和敌人的当前的平均速度，根据总路程、平均速度和三个传感器与墙的距离值计算出敌人的总体适应度，如果总体适应度太小或大得离谱，或者敌人的闲置时间太久，都认为这辆敌人失效，销毁这辆敌人。
///         2.5、在 FixedUpdate 每隔 0.02s 运行的过程当中，如果敌人发生碰撞，也会销毁敌人
///     
/// 
///     3、销毁敌人方法：Death()
///         1、在 Death 方法中会找到敌人的基因管理器 GeneticManager 脚本组件，并调用基因管理器的 Death 死亡方法，将这辆敌人的总体适应度 overallFitness 和 神经网络 network传递过去
///         2、基因管理器 GeneticManager 会经过一系列操作，最终调用基因管理器的 ResetToCurrentGenome 重置当前基因组方法，该方法会调用本对象的 ResetWithNetwork 方法传递一个新的神经网络过来重置当前的敌人
///         
/// 
///     4、根据神经网络重置敌人方法：ResetWithNetwork() 根据神经网络重置敌人神经网络方法
///         1、在基因管理器会调用 ResetWithNetwork 并传递一个新的神经网络过来重置敌人的神经网络
///         2、在 ResetWithNetwork 方法中重置敌人神经网络之后，要运行这个新的神经网络，就要重新设置这个敌人，所以在 ResetWithNetwork 方法中需要调用 Reset方法
///         
/// 
///     5、重置敌人方法：Reset() 
///         1、重置敌人的启动时间、总距离、平均速度、最后位置、总体适应度、开始位置和开始角度
///         2、重置敌人方法执行完毕后得到一个具有新的未运行过的神经网络的NPC，继续在 FixedUpdate 方法中逐帧运行神经网络
///         
///     6、需要理解的是：
///         1、在 FixedUpdate 运行的过程中，执行的 network.RunNetwork 方法发生了什么。所以要理解 NNet 神经网络脚本
///         2、死亡方法 Death() 调用了基因管理器 GeneticManager 脚本的死亡方法，所以要理解 GeneticManager 基因管理器脚本
/// </summary>
public class ANNController : MonoBehaviour
{
    #region 成员
    public Vector3 startPosition;//敌人开始时的位置
    private Vector3 lastPosition;//最终位置（敌人撞墙的位置）

    [Range(-1f, 1f)]
    public float x, y;//x 表示下一个位置的 x 坐标，y 表示下一个位置的 y 坐标
    public float timeSinceStart = 0f;//敌人逃跑状态启动来的时间，用来检测敌人是否闲置太久，如果闲置太久说明该神经网络失效


    [Header("Fitness")]
    public float overallFitness;//Fitness ：评估好坏神经网络好坏的指标
    public float totalDistanceMultipler = 6.4f;//总距离倍增器：首先重视距离，走得最远的敌人是我们要的。
    public float disFormDoorMultipler = 0.001f;//距离门的距离倍增器
    public float sensorMultiplier = 0.2f;//传感器倍增器：用来乘以传感器到墙的距离

    

    //与适应度相关的变量：总共走的距离越大并且离要到达的 Door 门的距离越小，并且离墙的距离越大，则说明适应度最好
    private float totalDistanceTravelled;//总共走的距离
    private float aSensor, bSensor, cSensor;//敌人的三个传感器到墙的距离：从传感器发出射线，射线于墙碰撞后，获取射线距离
    private float disFromDoor;//离门的距离


    private ANN network;//神经网络类对象
    [Header("Network Options")]
    public int LAYERS = 1;//神经网络层数
    public int NEURONS = 10;//神经网络的神经元节点数


    Transform doorTrans;//要逃往的门
    Rigidbody2D enemyRig;//刚体组件
    private Vector3 newPosition;//敌人下一帧所在的位置
    int enemyLayerMask;//过滤射线碰撞的层级
    #endregion






    #region 初始化
    /// <summary>
    /// 初始化：初始化开始位置和网络脚本
    /// </summary>
    private void Awake()
    {
        Debug.Log("初始化神经网络控制器");
        startPosition = transform.position;
        network = new ANN();//初始化神经网络脚本对象
        enemyRig = transform.GetComponent<Rigidbody2D>();
        doorTrans = GameObject.Find("Door2").transform;
        enemyLayerMask = LayerMask.NameToLayer("Enemy");//要过滤的射线检测层级
    }
    #endregion







    #region 更新
    /// <summary>
    /// 消除不同电脑带来的不同硬件环境，在固定的帧更新中执行
    /// </summary>
    private void FixedUpdate()
    {
        InputSensors();//获取传感器与墙的距离
        //Debug.Log("aSensor:" + aSensor.ToString() + "--------------bSensor:" + bSensor.ToString() + "------------cSensor:" + cSensor.ToString());
        lastPosition = transform.position;//得到当前位置，作为执行下一帧之前的最后一个位置
        disFromDoor = Vector3.Distance(transform.position, doorTrans.position);
        (x, y) = network.RunNetwork(aSensor, bSensor, cSensor);//通过运行神经网络，得到输出值
        Escape(x, y);//根据输出的 x 和 y 得到新的坐标
        timeSinceStart += Time.deltaTime;//累计时间
        CalculateFitness();//计算适应度
    }
    #endregion







    #region InputSensors 方法：根据射线检测获取三个传感器与墙的距离
    /// <summary>
    /// 获取传感器与墙的距离
    /// </summary>
    private void InputSensors()
    {

        Vector3 a = (transform.up + transform.right);//右前方向量
        Vector3 b = (transform.up);//正前方向量
        Vector3 c = (transform.up - transform.right);//左前方向量


        RaycastHit2D hitA = Physics2D.Raycast(transform.position, a, 200 , ~(1 << enemyLayerMask));
        if (hitA.collider)//如果射线r发生碰撞，将碰撞信息输出到hit内
        {
            aSensor = hitA.distance / 20;//a传感器距离墙的距离除以20,注意在输入到神经网络的时候需要对值进行标准化（需要确保输入神经网络的值总时在-1到1或0到1）
        }

        //得到 bSensor 和 cSensor 的值
        RaycastHit2D hitB = Physics2D.Raycast(transform.position, a, 200, ~(1 << enemyLayerMask));
        if (hitB.collider)
        {
            bSensor = hitB.distance / 20;
        }

        RaycastHit2D hitC = Physics2D.Raycast(transform.position, a, 200, ~(1 << enemyLayerMask));
        if (hitC.collider)
        {
            cSensor = hitC.distance / 20;
        }
    }
    #endregion









    #region 敌人移动方法
    

    /// <summary>
    /// 敌人移动方法
    /// </summary>
    /// <param name="v">敌人逃跑方向向量的 x 坐标</param>
    /// <param name="h">敌人逃跑方向向量的 y 坐标</param>
    public void Escape(float x, float y)
    {
        newPosition = Vector3.Lerp(Vector3.zero, new Vector3(x * 21.4f, y * 11.4f, 0), 0.02f);//根据伸进网络输出的 x 和 y 计算出每帧增加的距离
        newPosition = transform.TransformDirection(newPosition);//将当前物体方向向量转换到世界坐标系下
        newPosition.x = newPosition.x + transform.position.x;
        newPosition.y = newPosition.y + transform.position.y;
        transform.position = newPosition;
        
    }
    #endregion








    #region CalculateFitness 计算敌人总体适应度的方法
    /// <summary>
    /// 计算 overallFitness
    /// </summary>
    private void CalculateFitness()
    {

        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);//每一帧都将最后一个位置设置为当前位置，所以每一帧增加的距离就是当前位置和最后一个位置之间的距离
        //overallFitness = (totalDistanceTravelled * totalDistanceMultipler) + 1.0f / (disFromDoor * disFormDoorMultipler) + (3 / ((aSensor + bSensor + cSensor) * sensorMultiplier));
        overallFitness = 1 / (disFromDoor * disFormDoorMultipler) + totalDistanceTravelled * totalDistanceMultipler;
        if ((transform.position.x > 37 && transform.position.y > 13) || (transform.position.x < 48 && transform.position.y > 13))
        {
            //Debug.Log("成功越过障碍物并靠近逃脱口--------------------------------------------------------");
            //Debug.Log("当前适应度为：" + overallFitness.ToString());
        }
        if (timeSinceStart > 20 && overallFitness < 40)
        {//如果敌人闲置太久或整体适应度小于40，则说明此辆敌人已无效，销毁敌人
            //Debug.Log("因为敌人闲置太久或整体适应度小于40而死亡");
            Death();
            
        }
        if (overallFitness >= 1000)
        {//如果整体适应度过大，销毁敌人
            //Debug.Log("因为整体适应度过大而死亡:" + overallFitness.ToString());
            Death();
            
        }
    }
    #endregion







    #region OnCollisionEnter 敌人碰撞检测方法
    /// <summary>
    /// 碰撞方法：当碰撞时，执行死亡方法
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        Death();
    }
    #endregion







    #region 死亡方法
    /// <summary>
    /// 敌人死亡方法：将敌人的适应度和神经网络都传递给“基因控制器”脚本，将气保存到基因中
    /// </summary>
    public void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
    }
    #endregion






    #region 重置敌人方法
    /// <summary>
    /// 重置网络函数：当敌人撞墙，重置网络和敌人
    /// </summary>
    /// <param name="net"></param>
    public void ResetWithNetwork(ANN net)
    {
        network = net;
        Reset();
    }








    /// <summary>
    /// 重置敌人函数：当敌人撞墙，重置敌人
    /// </summary>
    public void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
    }
    #endregion



}
