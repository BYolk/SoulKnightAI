using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 神经网络类脚本思路：
///     1、密集矩阵 Dense Matrix：与稀疏矩阵对应，当一个矩阵中 0 元素的数量远远多于非 0 元素的数量，则该矩阵为稀疏矩阵，反之为密集矩阵。
///     
/// 
///     2、偏置项：相当于函数的截距 b，对应点斜式 y=kx+b ，y 相当于下一层的神经元，x 相当于当前层的神经元，k相当于权重，b 相当于偏置值。偏差像就是每个神经元的偏差值和偏差权重的乘积
///     
/// 
///     3、激活函数的作用：如果不使用激励函数，无论神经网络多么复杂，最后对应的函数在图上画出来都是线性的，即输出的都是一个线性组合。只有通过激励函数才能得到非线性的组合，可以用来表示非线性的图形，即激励函数的作用会大大增加神经网络的表达能力——激活函数会给神经元引入非线性元素
///         3.1、Sigmoid 激励函数：S型函数，也称为S型生长曲线，常用于神经网络中，将值映射到[0,1]区间上，定义为 y = 1 / (1 + e^-x)
///         3.2、Tanh 激励函数：双曲正切函数，将值映射到[-1,1]区间上，定义为 y = tanhx = sinhx / coshx = (e^x - e^-x) / (e^x + e^-x)
///     
/// 
///     4、每一个隐藏层神经元数量都相等
///     
/// 
///     5、变量：
///         5.1、神经网络由输入层、隐藏层和输出层组成，每一层都由矩阵表示。
///         5.2、因为输入层和输出层是确定的，输入层有三个输入神经元，分别输入敌人三个传感器距离墙的位置；输出层有两个输出神经元，分别输出敌人如何加速于转向。所以为输入层创建一个一行三列的密集矩阵，为输出层创建一个一行两列的密集矩阵。
///         5.3、因为隐藏层是不确定的，所以需要创建一个矩阵集合，用于保存每一个代表隐藏层的矩阵
///         5.4、每两个相邻的层之间具有权重，又因为隐藏层不确定，所以需要创建一个矩阵集合用来存放每一个代表权重的矩阵
///         5.5、除了输入层之外每一层都具有一个偏置值，所以需要创建一个集合来存放每一个代表偏置值的浮点数
///         5.6、最后需要一个浮点类型的变量来代表该神经网络的适应度
///         
///     
///     
/// 
///     6、RunNetwork(float a, float b, float c) 方法：该方法在 CarController 的 FixedUpdate 方法中每隔 0.02s 调用一次
///         6.1、因为该方法在 CarController 脚本的 FixedUpdate 方法中调用，所以本脚本中最先执行的方法就是 RunNetwork 方法
///         6.2、该方法需要传入三个参数，即敌人三个传感器距离障碍物的位置，代表神经网络输入层
///         6.3、将三个传入参数赋值到神经网络输入层的三个神经元
///         6.4、通过激活函数 Tanh 将输入层矩阵的值全部映射到[-1，1]区间上，为输入层引入非线性元素（注意此处不能使用 Sigmoid 激活函数，因为在输出之前，所有的值都跟敌人转向有关，而敌人转向需要用负数表示向左转
///         6.5、输入层矩阵 乘以 输入层与隐藏层的第一层之间的权重矩阵 + 隐藏层第一层的偏置值 = 隐藏层第一层矩阵，再将矩阵通过激活函数 Tanh 将第一层矩阵的值都映射到[-1，1]区间上，为其引入非线性元素
///         6.6、遍历隐藏层除了第一层之外的所有层级（因为第一层与输入层相连，需要额外处理），并同理获得每一个代表隐藏层的矩阵
///         6.7、根据隐藏层的最后一层，获得输出层的数据
///         6.8、返回输出层的输出数据，注意在返回之前需要通过激励函数处理输出数据，第一个数据代表加速度，需要通过 Sigmoid 将数据转化到[0,1]区间，第二个数据代表转向，需要通过 Tanh 将数据映射到[-1,1]区间
///         
/// 
///     7、Sigmoid 方法：
///         7.1、即激励函数 Sigmoid，因为插件 MathNet.Numerics 中只有激励函数 Tanh，所以需要额外编写 Sigmoid 
///         7.2、激励函数 Sigmoid 算法为：(1 / (1 + Mathf.Exp(-s)))，其中 s 表示要转换的值，即 x ，Mathf.Exp(s)：返回e的s次方
///         
/// 
///     8、NNet 脚本中所有方法的调用情况：
///         8.1、Sigmoid() 方法通过 RunNetwork() 方法调用，RunNetwork 方法在 CarContorller脚本的 FixedUpdate 方法中调用。
///         8.2、Initialise() 、RandomiseWeights() 这两个方法是用于初始化神经网络的方法，RandomiseWeights 方法在 Initialise 方法内调用，Initialise 方法在 GeneticManager 基因管理器中调用
///         8.3、InitialiseCopy() 、InitialiseHidden() 这两个方法用于拷贝并获得初始化神经网络的副本，InitialiseHidden 方法在 InitialiseCopy 方法内调用，InitialiseCopy 方法在 GeneticManager 基因管理器中调用
///         8.4、要充分理解神经网络初始化方法，还需要结合 GeneticManager 脚本的理解
///         
///         
///     
///     9、初始化神经网络方法：Initialise() 、RandomiseWeights()
///         9.1、Initialise(int hiddenLayerCount, int hiddenNeuronCount) ：
///             9.1.1、需要传递两个参数，一个是隐藏层数量，一个是隐藏层神经元数量
///             9.1.2、调用矩阵和集合的 Clear() 方法将输入层、隐藏层、输出层、权重和偏置项全部清空
///             9.1.3、循环“隐藏层数量 + 1”次：
///                 9.1.3.1、每循环一次创建一个密集矩阵，矩阵维度为“一行，‘隐藏层神经元节点数’列”，即根据给定的隐藏层数量和隐藏层的神经元数量创建相应数量的相应维度矩阵，并将这些矩阵添加到已经清空的隐藏层集合 hiddenLayers 中
///                 9.1.3.2、再根据隐藏层数量添加添加偏置值，偏置值从 -1 到 1 之间随机取一个浮点值；
///                 9.1.3.3、注意如果是第一层隐藏层，因为其与输入层相连，需要特殊处理，需要为其创建一个“三行，‘隐藏层神经元节点数’列”的密集矩阵用于存放权重，因为输入层为“一列，三行”的矩阵。将这个矩阵添加到“权重矩阵集合中”
///                 9.1.3.4、为其它隐藏层创建“‘隐藏层神经元节点数’行，‘隐藏层神经元节点数’列”矩阵用于存放除了第一层隐藏层之外的其它层的权重，并将这些矩阵添加到“权重矩阵集合中”
///             9.1.4、循环完毕之后，为输出层创建“‘隐藏层神经元节点数’行，2列”的矩阵用于存放最后一个隐藏层输出层的权重矩阵并将其添加到“权重矩阵集合中”，并为输出层添加偏置值，同样在 -1 到 1 之间随机取一个浮点值
///             9.1.5、调用 RandomiseWeights 方法为所有的权重值随机赋值
///         9.2、RandomiseWeights()：为权重随机赋值
///             9.2.1、遍历权重矩阵集合里面的所有权重矩阵
///             9.2.2、遍历每一个权重矩阵的每一行
///             9.2.3、遍历每一个权重矩阵的每一行的每一列
///             9.2.4、为权重矩阵的每一行的每一列的元素随机赋一个 -1 到 1 之间的浮点值
///         9.3、初始化神经网络的过程中为什么要对权重和偏置值都取随机值？
///             ——之后会创建大量的神经网络，每个神经网络都随机权重随机偏置值，然后经过训练从这些神经网络中筛选出表现最好的神经网络，即权重值和偏置值最合适的神经网络
///             
/// 
///     10、拷贝初始化神经网络副本方法：InitialiseCopy() 、InitialiseHidden() 
///         10.1、InitialiseCopy(int hiddenLayerCount, int hiddenNeuronCount) 方法：
///             10.1.1、需要传递两个参数：隐藏层数量与隐藏层的神经元数量
///             10.1.2、创建一个新的神经网络
///             10.1.3、为新的神经网络创建一个新的权重矩阵集合
///             10.1.4、遍历当前神经网络权重集合中的每一个权重矩阵，创建一个新的权重矩阵，矩阵维度为遍历到的权重矩阵，用于拷贝遍历到的权重矩阵。遍历矩阵的每一行每一列，将遍历到的权重矩阵的值对应赋值给新的权重矩阵，完成权重矩阵的拷贝，并将其添加到新的权重矩阵集合
///             10.1.5、创建新的偏置值集合对象，并将当前的偏置值集合拷贝到新的集合对象中
///             10.1.6、将拷贝出来的权重矩阵和偏置集合都赋值给新的神经网络
///             10.1.7、调用 InitialiseHidden 方法初始化圣经网络的隐藏层
///             10.1.8、将拷贝的神经网络副本返回给调用者，即 GeneticManager 基因管理器对象
///         10.2、InitialiseHidden(int hiddenLayerCount, int hiddenNeuronCount) 方法：
///             10.1.1、需要传递两个参数：隐藏层数量与隐藏层的神经元数量
///             10.1.2、清空神经网络的输入层、隐藏层和输出层
///             10.1.3、通过 for 循环为隐藏层创建相应数量的矩阵，并将矩阵添加到隐藏层矩阵集合中
///         10.3、通过 InitialiseCopy 和 InitialiseHidden 方法拷贝当前神经网络的所有权重值、偏置值和隐藏层数量结构，这是该神经网络的结构（输入值会根据敌人行动改变，通过这个神经网络结构的运行得到输出值，所以神经网络的拷贝不用设计输入层和输出层）
///             
/// 
///     11、注意：在 GeneticManager 脚本调用 Initialise 方法和 InitialiseCopy 方法时，需要传递两个参数，在这两个方法的定义中第一个参数是 hiddenLayerCount ，而在调用是传递过来的实参是 CarController 脚本对象的 LAYERS，即 LAYERS 赋值为 1 时，这两个方法在循环时会对 Layer + 1，即隐藏层数量为 2。
/// </summary>
public class ANN 
{
    #region 神经网络矩阵变量
    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);//输入层矩阵
    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();//隐藏层矩阵集合(可能存在多个隐藏层)
    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);//输出层矩阵
    public List<Matrix<float>> weights = new List<Matrix<float>>();//权重的矩阵集合
    public List<float> biases = new List<float>();//用于存放偏移值
    public float fitness;//适应度变量
    #endregion

    #region 更新，该方法会在 CarController 敌人控制器脚本中的 FixedUpdate 方法中每隔 0.02s 运行一次
    /// <summary>
    /// 运行神经网络方法
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns>返回一个一行二列的矩阵，分别表示敌人加速度和敌人转向</returns>
    public (float, float) RunNetwork(float a, float b, float c)
    {
        //设置输入层三个神经元的值
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;
        inputLayer = inputLayer.PointwiseTanh();//通过Tanh函数将每个节点的值转化到[-1，1]区间
        //这里为什么不将其通过Sigmoid函数转化到[0，1]，是因为输出值中x需要用负值表示左转

        //第一层隐藏层数据=输入层矩阵*权重矩阵+偏置值
        //再将第一层隐藏层数据通过Tanh函数转化到[-1，1]区间
        hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

        //遍历所有中间隐藏层（因为第一层隐藏层跟输入层相连，需要额外处理）
        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i - 1] * weights[i]) + biases[i]).PointwiseTanh();
        }

        //输出层数据=最后一层隐藏层矩阵*最后一层权重矩阵+最后一个偏差值
        //再将数据通过Tanh函数转化到[-1，1]区间
        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1]) + biases[biases.Count - 1]).PointwiseTanh();

        //第一个输出数据是x坐标，第二个输出数据是y坐标
        return ((float)Math.Tanh(outputLayer[0, 0]), Sigmoid(outputLayer[0, 1]));
    }
    #endregion







    #region Sigmoid 算法
    /// <summary>
    /// 将提供的值转化为0到1之间的值
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }
    #endregion








    #region 初始化神经网络方法：包括初始化神经网络方法
    /// <summary>
    /// 初始化神经网络：确保刚开始的时候，所有的值都是随机的（经过训练选出效果最好的权重值）
    /// </summary>
    /// <param name="hiddenLayerCount">初始化神经网络的隐藏层数量</param>
    /// <param name="hiddenNeuronCount">初始化神经网路的隐藏神经元（神经节点）数量</param>
    public void Initialise(int hiddenLayerCount, int hiddenNeuronCount)
    {
        //清除所有的神经网络输入层、隐藏层、输出层、权重和偏差值矩阵
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();


        //初始化神经网络隐藏层，偏移量与权重
        for (int i = 0; i < hiddenLayerCount + 1; i++)//遍历所有隐藏的神经网络隐藏层
        {
            //将每一个隐藏的神经网络隐藏层根据隐藏神经元数量保存到一个新矩阵中，并将其添加到神经网络的隐藏层矩阵变量 hiddenLayers 中
            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(f);
            biases.Add(Random.Range(-1f, 1f));//随机定义一个从-1到1的浮点数作为神经网络的偏差值

            //添加权重
            if (i == 0)//如果i==0，说明这一层的权重是输入层的权重，因为隐藏层第一层的左边是输入层
            {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCount);//因为输入层有三个神经元，所以行数是3，列数就是隐藏的神经元节点数量
                weights.Add(inputToH1);
            }
            //上面的权重是专门为输入层设计的权重
            //接下来的权重是为除了输入层之外的权重设计的（所有隐藏层都有相同的隐藏神经元数量）
            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(HiddenToHidden);
        }
        //设计输出层的权重矩阵
        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);//因为输出层只有两个神经元，所以列数是2
        weights.Add(OutputWeight);
        biases.Add(Random.Range(-1f, 1f));//同样添加-1到1的一个随机浮点数作为偏差值
        RandomiseWeights();//随机所有的权重值
    }




    /// <summary>
    /// 随机所有权重值：
    /// </summary>
    public void RandomiseWeights()
    {
        //遍历所有权重矩阵（所有权重矩阵都保存在集合内）
        for (int i = 0; i < weights.Count; i++)
        {
            //遍历每一个权重矩阵的行
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                //遍历每一个权重矩阵每一行的每一列
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = Random.Range(-1f, 1f);//从-1到1中随机一个浮点数作为该矩阵该行该列的值
                }
            }
        }
    }
    #endregion







    #region 复制初始化的神经网络方法——得到初始化神经网络的副本
    /// <summary>
    /// 初始化复制：初始化复制整个神经网络
    /// </summary>
    /// <param name="hiddenLayerCount">隐藏层数量</param>
    /// <param name="hiddenNeuronCount">隐藏层神经元数量</param>
    /// <returns></returns>
    public ANN InitialiseCopy(int hiddenLayerCount, int hiddenNeuronCount)
    {
        ANN n = new ANN();//创建一个新的神经网络
        List<Matrix<float>> newWeights = new List<Matrix<float>>();//创建一个新的神经网络权重矩阵数组

        //遍历当前每一个神经网络权重矩阵
        for (int i = 0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);//创建新的权重矩阵
            for (int x = 0; x < currentWeight.RowCount; x++)//遍历新的权重矩阵的每一行
            {
                for (int y = 0; y < currentWeight.ColumnCount; y++)//遍历新的权重矩阵的每一列
                {
                    currentWeight[x, y] = weights[i][x, y];//给新的权重矩阵的每行每列的元素赋值，赋的值是当前要拷贝的权重矩阵对应的元素值
                }
            }
            newWeights.Add(currentWeight);//将新的权重矩阵添加到新的权重矩阵集合中
        }

        List<float> newBiases = new List<float>();//创建新的偏差值集合

        newBiases.AddRange(biases);//将原本的偏差值集合赋值给新的偏差值集合
        n.weights = newWeights;//拷贝权重给新的神经网络
        n.biases = newBiases;//拷贝偏差值给新的神经网络
        n.InitialiseHidden(hiddenLayerCount, hiddenNeuronCount);//初始化神经网络的隐藏层
        //完成神经网络的拷贝
        return n;//返回拷贝号的神经网络
    }




    /// <summary>
    /// 初始化神经网络的隐藏层
    /// </summary>
    /// <param name="hiddenLayerCount">隐藏层数量</param>
    /// <param name="hiddenNeuronCount">隐藏层的神经元数量</param>
    public void InitialiseHidden(int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();//清除神经网络的隐输入层
        hiddenLayers.Clear();//清除神经网络的隐藏层
        outputLayer.Clear();//清除神经网络的输出层


        //遍历每一个神经网络隐藏层
        for (int i = 0; i < hiddenLayerCount + 1; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);//为每一个神经网络隐藏层创建矩阵用于存放神经元
            hiddenLayers.Add(newHiddenLayer);//将矩阵添加到隐藏层中
        }

    }

    #endregion



}
