using MathNet.Numerics.LinearAlgebra;

namespace ParallelExpressions.Core.Services
{
    public static class MatrixMapReduceFuncs
    {
        public static Func<List<FuncExpression>, object> Add = (ex) =>
        {
            Thread.Sleep(1000);

            if (ex.Count == 0)
            {
                return null;
            }

            if (ex.Count == 1) 
            {
                return (Matrix<double>)ex[0].Data;
            }

            var result = (Matrix<double>)ex[0].Data;

            for (int i = 1; i < ex.Count; i++)
            {
                result = result.Add((Matrix<double>)ex[i].Data);
            }

            return result;
        };

        public static Func<List<FuncExpression>, object> Multiply = (ex) =>
        {
            Thread.Sleep(1000);

            if (ex.Count == 0)
            {
                return null;
            }

            if (ex.Count == 1)
            {
                return (Matrix<double>)ex[0].Data;
            }

            var result = (Matrix<double>)ex[0].Data;

            for (int i = 1; i < ex.Count; i++)
            {
                result = result.Multiply((Matrix<double>)ex[i].Data);
            }

            return result;
        };
    }
}
