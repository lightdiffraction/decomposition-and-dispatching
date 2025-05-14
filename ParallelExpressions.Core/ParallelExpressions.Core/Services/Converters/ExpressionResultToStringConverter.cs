using MathNet.Numerics.LinearAlgebra;
using System.Text;

namespace ParallelExpressions.Core.Services.Converters
{
    public static class ExpressionResultToStringConverter
    {
        public static string Convert(object data)
        {
            var matrix = (Matrix<double>)data;

            if (matrix == null) 
            {
                return string.Empty;
            }

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    result.Append($"{matrix[j, i].ToString()} ");
                }
            }

            return result.ToString();
        }
    }
}
