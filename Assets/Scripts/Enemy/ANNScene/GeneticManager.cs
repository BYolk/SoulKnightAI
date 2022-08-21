using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

/// <summary>
/// 基因管理器脚本思路：使用遗传算法；在场景中创建空物体并挂载此脚本（不是挂载到敌人对象上）
///     1、相关概念：
///         1.1、遗传算法：类似自然选择，在每一代之中选取表现最好的个体，让他们通过杂交繁殖得到子代，而且子代还有概率发生突变，增加子代的多样性
///         
/// 
/// 
///     2、变量：
///         2.1、initialPopulation：定义每一代的人口数量（种群数量）
///         2.2、mutationRate：突变概率，随机获取一个值，小于这个突变概率则认为发生突变
///         2.3、bestAgentSelection：设置从当代中获取性能表现最好的个体数
///         2.4、worstAgentSelection：设置从当代中获取性能表现最差的个体数
///         2.5、numberToCrossover：设置选取出来的个体的交叉繁殖的次数
///         2.6、controller：当前正在运行的个体的控制器脚本对象（为什么不是创建数组来表示种群数量——我们关注的是神经网络，即关注的是 NNet 脚本，而不是 controller 脚本，只要为 controller 更换不同的 NNet 脚本，就可以表示不同的种群个体
///         2.7、genePool：基因库，此变量的目的不好解释，结合代码更好理解
///         2.8、naturallySelected：自然选择的数量，这个变量的目的不好说明，结合代码更好理解
///         2.9、population：种群，即人口数量，类型是 NNet 数组，用不同的 NNet 脚本即不同的神经网络表示不同的个体（筛选表现好的个体其实就是筛选表现好的神经网络）
///         2.10、currentGeneration：当前为第几代
///         2.10、currentGenome：当前的基因组，即当前为第几个种群个体，或者说当前为第几个神经网络
///         
/// 
///     3、场景加载挂载有此脚本的游戏对象，执行此脚本的Start方法进行初始化，在Start方法中调用 CreatePopulation 方法创建种群
///     
/// 
/// 
///     4、CreatePopulation() 创建种群方法：
///         1、创建一个 ANN 数组，数组大小是定义好的“初始化种群数量”，此处初始化种群数量为 85，即初始化 85 个 NNet 脚本对象
///         2、调用 FillPopulationWithRandomValues 方法，为神经网络数组赋值（创建神经网络对象保存到神经网络数组中）
///         3、调用 ResetToCurrentGenome 方法，重置当前基因组（将神经网络数组下标为 currentGenome 的神经网络传递给 CarController 脚本对象，用于取代敌人的神经网络（更换敌人的神经网络）
///         
/// 
/// 
///     5、void FillPopulationWithRandomValues(NNet[] newPopulation, int startingIndex) 使用随机数填充种群方法：
///         1、该方法需要两个参数，一个是需要填充的种群数组，即神经网络数组；一个是整数型下标，表示从神经网络数组的哪一个开始填充
///         2、使用 while 语句进行循环，当 startingIndex 下标位置大于等于种群数量时，停止循环
///         3、在循环过程中，实例化种群个体，即实例化神经网络，new 一个 NNet 脚本对象，赋予 startingIndex 位置处的数组元素，并初始化每一个神经网络，即初始化每一个神经网络的隐藏层、权重和偏置值（权重和偏置值是随机的），需要传递已经设定好的隐藏层数量和隐藏层神经元数量（controller 中设定的隐藏层数量和隐藏层神经元数量）
///         
/// 
/// 
///     6、ResetToCurrentGenome() 重置当前基因组方法，该方法会调用 CarController 的 ResetWithNetwork 方法，传递一个种群个体（神经网络）到 CarController 脚本对象，用来取代 CarController 脚本对象当前的神经网络。
///     
/// 
/// 
///     7、执行 Start 方法后，挂载有此脚本的空对象物体会有一个长度为 85(initialPopulation) 的神经网络数组，并通过 FillPopulationWithRandomValues 方法创建 85 个神经网络对象填充到神经网络数组中，因为 currentGenome 初始值为0，Start 方法会调用 ResetToCurrentGenome 方法， ResetToCurrentGenome 方法会执行 controller.ResetWithNetwork(population[currentGenome]) 即 controller.ResetWithNetwork(population[0]) 将神经网络数组的第一个神经网络传递到 controller 脚本对象中，并用它作为 controller 脚本对象的神经网络————需要注意的是，controller 脚本对象在接受 GeneticManager 脚本传递过去的神经网络数组的第一个神经网络对象之前，本身已经包含有一个神经网络对象，这个神经网络对象在 controller 脚本的 Awake 方法中实例化，而 GeneticManager 脚本实例化神经网络数组代码是在 Start 方法中执行的。
///     
/// 
/// 
///     8、本脚本除了场景加载就会运行的 Start 方法以及在 Start 方法调用的方法之外，剩下的最先执行的方法就是当敌人对象销毁时——可能时发生碰撞销毁，可能时适应度太小或过分的大而销毁，也可能时闲置时间过久而销毁，CarController 脚本对象会调用 Death 方法，而该方法又会调用本脚本的 Death() 方法
///     
/// 
/// 
///     9、Death()方法：
///         9.1、在程序开始的时候，GeneticManager 脚本会将实例化的神经网络数组的第一个神经网络对象传递给 Contorller 脚本对象进行神经网络训练，并将当前在训练的神经网络的下标记录到 currentGenome 中，所以 currentGenome 初始化为0。当 Controller 脚本调用 Death 方法时，表明第一个神经网络训练完毕，Controller 的 Death 方法会调用本脚本的 Death 方法，并传递该神经网络的训练结果 fitness 过来
///         9.2、本脚本的 Death 方法会判断当前正在训练的神经网络时神经网络数组的第几个神经网络，因为 currentGenome 为0，表示当前结束训练的神经网络是神经网络数组的第1个也就是下标为0的神经网络，说明该神经网络数组还有 84 个神经网络没有训练，所以先将当前结束训练的神经网络的训练结果即该神经网络的 fitness 保存到该神经网络对象的 fitness 变量当中。
///         9.3、保存好当前结束训练的神经网络的训练结果后，currentGenome 自增1，即将神经网络数组下标加1，将索引移到该下一个未训练的神经网络上，并调用 ResetToCurrentGenome() 方法将未训练的神经网络传递到 CarController 脚本对象，替代已经结束训练的神经网络，然后 FixedUpdate 方法会继续训练该神经网络
///         9.4、以此类推
///         9.5、当 CarController 训练的神经网络是 GeneticManager 神经网络数组中的最后一个元素时，即 currentGenome = population.Length - 1，表明 GeneticManager 神经网络数组已经全部训练完毕，即当代的种群已经全部训练完毕，接下来要进入下一代种群的训练，调用 RePopulate() 方法重置种群获得下一代种群（神经网络数组）（遗传算法的精髓在于下一代种群根据上一代种群遗传与变异而来）
/// 
/// 
/// 
///     10、Repopulate()：
///         10.1、在重置种群即重置神经网络数组时，需要调用集合的 .Clear 方法将 genePool 基因池（基因库）集合清空（确保 genePool 集合为空），后续要用来添加很多很多的神经网络对象
///         10.2、naturallySelected 赋值为 0，这个变量的目的不好说明，结合代码更好理解
///         10.3、currentGeneration 自增1，表示进入下一代种群训练
///         10.4、调用 SortPopulation() 方法对上一代种群进行排序，将性能表现最好的个体（神经网络）排在最前面，最差的排在最后面
///         10.5、调用 PickBestPopulation() 方法将性能表现最好和最差的前几个个体（神经网络）保存到 newPopulation 数组中
///         10.6、调用 Crossover(newPopulation)方法对保存起来的性能最突出的前几个个体进行交叉繁殖，获得第二代个体（神经网络）
///         10.7、调用 Mutate(newPopulation) 方法，繁殖的个体概率进行基因突变（随机改变权重值、偏置值）
///         10.8、调用 FillPopulationWithRandomValue() 方法，从0开始实例化 newPopulation 数组
///         10.9、将 newPopulation 替代当前的 population，即替代当前的种群（神经网络数组）
///         10.10、将 currentGenome 当前训练的神经网络数组下标重置为0
///         10.11、调用 ResetToCurrentGenome() 方法，将当前新一代的神经网络数组的第一个神经网络对象传递给 CarController 对象进行训练
///         10.12、开始第二代神经网络训练……
///         
/// 
/// 
///     11、SortPopulation()：使用冒泡排序算法将已经训练完毕的当代种群（神经网络数组） population 按照性能指标 fitness 从大到小排序
///     
/// 
///     12、NNet[] PickBestPopulation()：挑选性能表现最好的和最差的几个个体（神经网络）并以数组形式返回
///         12.1、创建一个新的种群变量（神经网络数组变量） newPopulation
///         12.2、循环 bestAgentSelection 次，bestAgentSelection初始化为8，即循环8次，将种群（神经网络数组）中表现最好的 8 个个体挑选出来保存到 newPopulation 中，因为种群已经根据性能表现排序好，所以挑选的个体（神经网络）就是种群（神经网络数组）中下标为 0 到 7 的元素，挑选到的个体下标用 naturallySelected 来表示（被挑选到的个体就是自然选择的结果），这也是为什么在重置种群算法中需要把 naturallySelected 置为0。
///         12.3、在循环过程中调用神经网络对象的 InitialiseCopy 方法拷贝当前的神经网络，得到当前神经网络的副本，并将该神经网络副本的适应度 fitness 设置为0，然后保存到 newPopulation 的相应位置， naturallySelected 再自增 1
///         12.4、接下来要做的事情的理由是：挑选出来的对象要用来交叉繁殖，交叉繁殖的对象都放在 genePool 里面，因为性能表现好的对象被选中繁殖的概率要大，所以 genePool 中性能表现好的个体数量就要越多，并用循环的下标值 i 代表该神经网络对象。具体的做法就是将该神经网络的 fitness * 10，然后转为Int类型数字，记为 f，然后以 f 为循环条件循环 f 此，每循环一次将 i 添加到 genePool 中（性能表现好的 fitness 值就高，乘以10之后的数就越大，循环的次数就越多，代表该神经网络的 i 值被添加到 genePool 的次数就越多。
///         12.5、同理循环 worstAgentSelection(3) 次，挑选出性能表现最差的 3 个个体（神经网络），但不将这些个体保存到 newPopulation 中，而是将代表性能最差个体的下标值 last 添加到 genePool 里面，添加方法同理。
///         12.6、挑选完毕，返回 newPopulation（此时 newPopulation 里面有八个神经网络对象，是当代训练完毕后表现最好的八个神经网络）
///         12.7、需要注意的是，挑选到 newPopulation 集合中的只有性能表现最好的神经网络对象，而没有性能表现最差的神经网络对象。把性能表现最差的神经网络对象的下标值保存到 genePool 的原因是，在交叉繁殖方法 Crossover 中，会概率从 8 个最好的孩子和 3 个最差的孩子总共 11 个孩子中随机挑选两个（这两个可能包括自身，即自交）进行繁殖。
///         
/// 
/// 
///     13、void Crossover(NNet[] newPopulation)：交叉繁殖方法
///         13.1、newPopulation 神经网络数组参数：要进行交叉繁殖的神经网络数组
///         13.2、循环 numberToCrossover 次，即交叉繁殖次数，最后会获得
///         13.3、在循环过程中定义两个下标 Aindex 和 Bindex，然后嵌套循环100次，在 0 到 genePool 集合长度中随机获取一个整数，作为 genePool 集合的索引下标，即随机从 genePool 中取出两个数赋值给 Aindex 和 Bindex，只要这两个数不相等，立马跳出循环，除非循环 100 次从 genePool 中随机取到的值都是相等的。之后会根据这两个下标在当前种群（神经网络数组）中获取对应的个体对象（神经网络对象）
///         13.4、创建两个神经网络对象 Child1 和 Child2，表示孩子1和孩子2，并调用神经网络对象的 Initialise 方法初始化这两个神经网络对象，并初始化两个神经网络的适应度 fitness 为 0
///         13.5、遍历两个孩子对象的权重矩阵集合，在每一个权重矩阵中，孩子1有一半的概率接受来自 Aindex 坐标的神经网络对象的权重矩阵，一半的概率来自 Bindex 坐标的神经网络对象的权重矩阵，孩子2与孩子1对应相反。除非 AIndex 和 BIndex值一样。
///         13.6、同理遍历两个孩子对象的偏置值集合并概率赋予
///         13.7、将孩子1和孩子2保存到 newPopulation 集合中，每保存一个让 naturallySelected 自增1（naturallySelected 即新一代种群（新一代神经网络数组）的个体数量（神经网络对象个数、数组长度）
///         
/// 
/// 
/// 
///     14、void Mutate(NNet[] newPopulation)：基因突变方法
///         14.1、循环 naturallySelected 次数，naturallySelected 表示已经添加到 newPopulation 集合中的神经网络个数，即遍历 naturallySelected 集合中的每一个神经网络对象
///         14.2、嵌套循环遍历每一个神经网络对象的权重矩阵集合，即遍历每一个权重矩阵
///         14.3、随机获取一个 0.0f 到 1.0f 的随机浮点数，如果这个数小于 mutationRate 突变率，则认为该神经元发生突变，切确的说是权重矩阵发生突变，调用 MutateMatrix 突变矩阵方法将“突变”该矩阵，将突变后的矩阵替代当前矩阵
///         
/// 
///         
///     15、Matrix<float> MutateMatrix(Matrix<float> A)：突变矩阵方法
///         15.1、传递一个矩阵过来，将该矩阵进行“突变”，将突变后的矩阵返回
///         15.2、定义随机突变点 randomPoints = Random.Range(1,(A.RowCount * A.ColumnCount) / 7);具体的值是根据实验调整确定的
///         15.3、定义一个新矩阵变量，并赋予将要突变的矩阵的值
///         15.4、以 randomPoints 作为循环条件循环，每一次循环中随机定义发生突变的行数和列数，根据行数与列数锁定发生突变的元素，为该突变的元素随机加上介于 -1 到 1 的一个浮点数，并用 Mathf.Clamp 方法将该元素的值锁定在 [-1,1] 区间，即随机加上浮点数后如果小于 -1 则返回 -1，大于 1 则返回 1。
///         15.5、返回突变后的矩阵
///         
/// 
/// 
///     
///     
/// </summary>
public class GeneticManager : MonoBehaviour
{
    #region 成员
    [Header("References")]
    public ANNController controller;//获取敌人控制器

    [Header("Controls")]
    public int initialPopulation = 200;//初始化人口数量
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;//突变率：神经网络随机化的机率

    [Header("Crossover Controls")]
    public int bestAgentSelection = 10;//性能最佳的敌人
    public int worstAgentSelection = 0;//性能最差的敌人
    public int numberToCrossover;//杂交次数
    private List<int> genePool = new List<int>();//基因池（库）：包含所有已选择的网络（即上面11辆敌人会以不同数量添加到基因库）
    private int naturallySelected;//自然选择

    private ANN[] population;//人口变量，每一个人都有一个神经网络，所以类型是神经网路类型

    [Header("Public View")]//用于调试
    public int currentGeneration;//当前第几代
    public int currentGenome = 0;//当前基因组
    public int exceptSuccessed = 0;//成功逃脱次数
    #endregion







    #region 初始化
    /// <summary>
    /// 初始化：创建种群
    /// </summary>
    private void Start()
    {
        controller = GameObject.Find("Ann_Enemy_0_AngryPig").GetComponent<ANNController>();
        CreatePopulation();//创建人口
    }




    /// <summary>
    /// 创建人口种群
    /// </summary>
    private void CreatePopulation()
    {
        population = new ANN[initialPopulation];//每一个人口都有一个神经网络
        FillPopulationWithRandomValues(population, 0);//使用随机值填充人口，然后会将第一个基因组分配给当前敌人，所以要重置当前基因组
        ResetToCurrentGenome();//重置当前基因组
    }
    #endregion




    #region 使用随机数填充种群：实例化种群（神经网络）添加到种群（神经网络）数组中
    /// <summary>
    /// 使用随机数填充人口
    /// </summary>
    /// <param name="newPopulation">要填充的人口</param>
    /// <param name="startingIndex">开始填充的下标位置</param>
    private void FillPopulationWithRandomValues(ANN[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new ANN();//为每个人添加神经网络
            newPopulation[startingIndex].Initialise(controller.LAYERS, controller.NEURONS);//初始化每个神经网络（初始化添加的值就是随机的）
            startingIndex++;
        }
    }
    #endregion





    #region 重置当前基因组
    /// <summary>
    /// 重置敌人神经网络（基因组）
    /// </summary>
    private void ResetToCurrentGenome()
    {
        controller.ResetWithNetwork(population[currentGenome]);//重置敌人神经网络和敌人本身
    }
    #endregion






    #region 死亡方法
    /// <summary>
    /// 死亡方法：一旦敌人死亡，将自身的适应度加入网络（保存每一个适应度），并调用下一个种群个体（神经网络）
    /// </summary>
    /// <param name="fitness"></param>
    /// <param name="network"></param>
    public void Death(float fitness, ANN network)
    {
        if (currentGenome < population.Length - 1)//如果神经网络数组下标小于神经网络数组长度-1，说明还有神经网络没有经过训练，将数组的下一个神经网络传递到 CarController 脚本对象当中进行训练
        {
            population[currentGenome].fitness = fitness;//将该神经网络训练的结果即适应度保存到该神经脚本的 fitness 变量中
            currentGenome++;//基因组自增1，即神经网络数组下标自增1
            ResetToCurrentGenome();//将下一个神经网络传递给 CarController，重置敌人的神经网络，并在 FixedUpdate 方法中继续运行神经网络
        }
        else//代码走这里说明当前神经网络数组全部训练完毕，即这一代全部训练完毕，重置种群进入下一代
        {
            RePopulate();//重置人口
        }
    }
    #endregion






    #region 重置人口方法
    /// <summary>
    /// 重置人口
    /// </summary>
    private void RePopulate()
    {
        genePool.Clear();//清除基因库
        currentGeneration++;//进入下一代
        naturallySelected = 0;//重置自然选择 
        SortPopulation();//对人口进行排序（将性能最好的敌人排在前面，用来选出最好的人口）

        ANN[] newPopulation = PickBestPopulation();//挑选最好的人口
        Crossover(newPopulation);//交叉繁殖
        Mutate(newPopulation);//基因突变

        FillPopulationWithRandomValues(newPopulation, naturallySelected);//随机值填充人口
        population = newPopulation;//将重置的人口赋予当前人口

        currentGenome = 0;//基因组置为0
        ResetToCurrentGenome();//重置当前基因组
    }
    #endregion






    #region 排序算法：将已经训练完毕的当代种群（神经网络数组）按照性能指标 fitness 从大到小排序
    /// <summary>
    /// 使用冒泡算法排序人口
    /// </summary>
    private void SortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)//将第i个和第i个之后的每一个人进行对比，将适应度高的往前排
                {
                    ANN temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }
    #endregion






    #region 挑选方法：从排序好的种群(神经网络数组)中挑选性能表现最好的个体存放到一个新的种群变量(神经网络数组变量)当中并返回
    /// <summary>
    /// 挑选表现最好的人口
    /// </summary>
    /// <returns></returns>
    private ANN[] PickBestPopulation()
    {

        ANN[] newPopulation = new ANN[initialPopulation];//定义一个新的种群

        //往性能表现最好的人口种群数组中添加表现最好的人
        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitialiseCopy(controller.LAYERS, controller.NEURONS);//复制神经网络隐藏层添加到新人口对象
            newPopulation[naturallySelected].fitness = 0;//将新人口对象的适应度设置为0
            naturallySelected++;//自然选择自增

            int f = Mathf.RoundToInt(population[i].fitness * 10);//适应度越高的人，乘以10之后值就越大，值越大，循环次数就越多，被添加到基因库中的次数就越多
            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        //遍历性能表现最差的人口种群
        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;//从人口种群的尾部开始选择性能表现最差的人
            last -= i;

            //性能表现最差的适应度最低，乘10的值比起其它表现好的要小很多，循环次数也相应小很多，被添加到基因池的次数也就小很多
            int f = Mathf.RoundToInt(population[last].fitness * 10);
            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }

        }
        return newPopulation;//返回新的人口种群（里面包含的是上一代种群中表现最好和最差的基因组）
    }
    #endregion







    #region 交叉繁殖方法,每交叉繁殖一次会获得两个子对象
    /// <summary>
    /// 交叉得到新的孩子（杂交繁殖）
    /// </summary>
    /// <param name="newPopulation"></param>
    private void Crossover(ANN[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i += 2)//循环杂交次数
        {
            //第一个和第二个进行繁殖
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)//只有基因库数量大于等于1即至少有两个对象才可以进行繁殖
            {
                for (int l = 0; l < 100; l++)//循环100次，获得两个不同的下标
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];//从基因库中随机得到一个下标
                    BIndex = genePool[Random.Range(0, genePool.Count)];//从基因库中随机得到一个下标

                    if (AIndex != BIndex)//如果得到的两个下标不同
                        break;//跳出循环
                }
            }

            //创建两个新的孩子对象
            ANN Child1 = new ANN();
            ANN Child2 = new ANN();

            //初始化两个孩子对象
            Child1.Initialise(controller.LAYERS, controller.NEURONS);
            Child2.Initialise(controller.LAYERS, controller.NEURONS);

            //初始化孩子对象的适应度
            Child1.fitness = 0;
            Child2.fitness = 0;

            //遍历孩子对象的每一个权重并随机赋值
            for (int w = 0; w < Child1.weights.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)//随机获得0到1的数，如果小于0.5
                {
                    Child1.weights[w] = population[AIndex].weights[w];//将从种群中随机获得的AIndex人口的权重赋给第一个孩子
                    Child2.weights[w] = population[BIndex].weights[w];//将从种群中随机获得的BIndex人口的权重赋给第二个孩子
                }
                else//如果随机数大于0.5，则交换AIndex和BIndex赋值权重给两个孩子
                {
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                }

            }

            //遍历孩子对象的每一个偏差项并随机赋值
            for (int w = 0; w < Child1.biases.Count; w++)
            {

                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                }

            }
            //将孩子对象保存到新的人口种群中
            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            naturallySelected++;

        }
    }
    #endregion







    #region 基因突变方法
    /// <summary>
    /// 基因突变方法：基因突变，增加不确定性
    /// </summary>
    /// <param name="newPopulation"></param>
    private void Mutate(ANN[] newPopulation)
    {

        for (int i = 0; i < naturallySelected; i++)//循环自然选择数
        {
            for (int c = 0; c < newPopulation[i].weights.Count; c++)//循环每一个人口的每一个权重
            {

                if (Random.Range(0.0f, 1.0f) < mutationRate)//从0到1中随机获得一个浮点数，如果该浮点数小于突变率
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);//调用突变矩阵方法获得突变权重，并将其赋予该人口，表示该人口对象发生突变
                }

            }
        }
    }


    /// <summary>
    /// 突变矩阵方法：对一个矩阵的值进行改变
    /// </summary>
    /// <param name="A">发生突变的矩阵</param>
    /// <returns>返回一个发生突变的矩阵</returns>
    Matrix<float> MutateMatrix(Matrix<float> A)
    {
        //随机突变点
        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);//根据实验调整最终确定的

        Matrix<float> C = A;//定义一个新的矩阵，并把为突变的矩阵赋值给它

        //循环随机突变点
        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);//定义发生突变的随机列数
            int randomRow = Random.Range(0, C.RowCount);//定义发生突变的随机行数

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);//突变
        }

        return C;

    }
    #endregion


}
