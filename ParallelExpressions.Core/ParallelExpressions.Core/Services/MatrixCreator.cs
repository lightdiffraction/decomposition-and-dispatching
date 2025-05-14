using MathNet.Numerics.LinearAlgebra;

namespace ParallelExpressions.Core.Services
{
    public static class MatrixCreator
    {
        public static Matrix<double> CreateMatrix1() 
        {
            double[][] ar = new double[3][];

            ar[0] = new double[] { 1, 2, 3, 4, 5 };
            ar[1] = new double[] { 5, 3, 3, 2, 1 };
            ar[2] = new double[] { 3, 4, 4, 5, 2 };

            Matrix<double> matrix = Matrix<double>.Build.DenseOfColumns(ar);
            return matrix;
        }

        public static Matrix<double> CreateMatrix2()
        {
            double[][] ar = new double[3][];

            ar[0] = new double[] { 1, 2, 1, 4, 1 };
            ar[1] = new double[] { 5, 3, 1, 2, 1 };
            ar[2] = new double[] { 3, 4, 1, 5, 1 };

            Matrix<double> matrix = Matrix<double>.Build.DenseOfColumns(ar);
            return matrix;
        }

        public static Matrix<double> CreateMatrix3()
        {
            double[][] ar = new double[3][];

            ar[0] = new double[] { 1, 2, 1, 1, 1 };
            ar[1] = new double[] { 5, 3, 1, 1, 1 };
            ar[2] = new double[] { 3, 4, 1, 1, 1 };

            Matrix<double> matrix = Matrix<double>.Build.DenseOfColumns(ar);
            return matrix;
        }

        public static Matrix<double> CreateMatrix4()
        {
            double[][] ar = new double[3][];

            ar[0] = new double[] { 1, 1, 1, 4, 1 };
            ar[1] = new double[] { 5, 1, 1, 2, 1 };
            ar[2] = new double[] { 3, 1, 1, 5, 1 };

            Matrix<double> matrix = Matrix<double>.Build.DenseOfColumns(ar);
            return matrix;
        }

        public static Matrix<double> CreateMatrix5()
        {
            double[][] ar = new double[3][];

            ar[0] = new double[] { 1, 1, 1, 4, 1 };
            ar[1] = new double[] { 1, 1, 1, 2, 1 };
            ar[2] = new double[] { 1, 1, 1, 5, 1 };

            Matrix<double> matrix = Matrix<double>.Build.DenseOfColumns(ar);
            return matrix;
        }

        public static Matrix<double> CreateMatrix6()
        {
            double[][] ar = new double[3][];

            ar[0] = new double[] { 1, 1, 1, 1, 1 };
            ar[1] = new double[] { 5, 1, 1, 1, 1 };
            ar[2] = new double[] { 3, 1, 1, 1, 1 };

            Matrix<double> matrix = Matrix<double>.Build.DenseOfColumns(ar);
            return matrix;
        }
    }
}
