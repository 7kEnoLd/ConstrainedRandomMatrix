using Gurobi;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConstrainedRandomMatrix
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // 创建空环境，设置参数并开始
                GRBEnv env = new(true);
                env.Set("Logfile", "ILP.log");
                env.Start();

                // 创建一个空模型
                GRBModel model = new(env)
                {
                    ModelName = "RandomMatrix"
                };

                // 决策变量,目标函数 一个0-1的随机数
                GRBVar[,] x = new GRBVar[475, 132]; // 475*132
                for (int i = 0; i < 475; i++)
                {
                    for (int j = 0; j < 132; j++)
                    {
                        double lb = 0;
                        double ub = 0; // 上下界

                        Random random = new();
                        double f = random.NextDouble();

                        x[i, j] = model.AddVar(lb, double.MaxValue, 0, GRB.CONTINUOUS, "x" + i + "_" + j);
                    }
                }


                // 约束条件1：行约束，每行的和等于500
                for (int i = 0; i < 475; i++)
                {
                    GRBLinExpr rowRestrict = 0.0;
                    for (int j = 0; j < 132; j++)
                    {
                        rowRestrict.AddTerm(1.0, x[i, j]);
                    }
                    model.AddConstr(rowRestrict == 500, "rc：" + (i + 1).ToString());
                }

                // 约束条件1：列约束，每列的和小于等于800
                for (int i = 0; i < 132; i++)
                {
                    GRBLinExpr columnRestrict = 0.0;
                    for (int j = 0; j < 475; j++)
                    {
                        columnRestrict.AddTerm(1.0, x[j, i]);
                    }
                    model.AddConstr(columnRestrict <= 800, "cc：" + (i + 1).ToString());
                }

                // 设置目标类型，并优化
                model.ModelSense = GRB.MAXIMIZE;
                model.Optimize();

                // 获取结果
                double[,] result = new double[475, 132];
                for (int i = 0; i < 475; i++)
                {
                    for (int j = 0; j < 132; j++)
                    {
                        result[i, j] = x[i, j].X;
                    }
                }
                double objVal = model.ObjVal;

                // 释放模型和环境的非托管资源
                model.Dispose();
                env.Dispose();
            }
            catch (GRBException e)
            {
                throw new Exception("Error code:" + e.ErrorCode + "." + e.Message);
            }
        }
    }
}
