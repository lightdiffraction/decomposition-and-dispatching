using MathNet.Numerics.LinearAlgebra;

namespace ParallelExpressions.Core.Services.Models
{
    public class Matrix
    {
        public Matrix<double> Coordinates { get; set; }

        public string Operand { get; set; }
    }
}
