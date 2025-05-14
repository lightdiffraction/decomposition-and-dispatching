using MathNet.Numerics.LinearAlgebra;

namespace ParallelExpressions.Core.Services.Mappers
{
    public class MatrixMapper
    {
        public Models.Matrix Map(Core.Models.Matrix matrix)
        {
            int rows = matrix.Rows;
            int columns = matrix.Columns;

            var coordinatesArray = matrix.Coordinates.Split(" ");

            int k = 0;

            var coordinates = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                coordinates[i] = new double[columns];
                for (int j = 0; j < columns; j++) 
                {
                    coordinates[i][j] = double.Parse(coordinatesArray[k++]);
                }
            }

            var result = new Models.Matrix()
            {
                Operand = matrix.Operand,
                Coordinates = Matrix<double>.Build.DenseOfColumns(coordinates),
            };

            return result;
        }
    }
}
