using ParallelExpressions.Core.Services.Models;
using Matrix = ParallelExpressions.Core.Models.Matrix;

namespace ParallelExpressions.Core.Services.Mappers
{
    public class MatrixResponseMapper
    {
        public MatrixResponse Map(Matrix matrix)
        {
            int rows = matrix.Rows;
            int columns = matrix.Columns;

            var coordinatesArray = matrix.Coordinates.Split(" ");

            int k = 0;

            var result = new MatrixResponse();

            for (int i = 0; i < rows; i++)
            {
                result.Rows.Add(new MatrixRow());
                for (int j = 0; j < columns; j++)
                {
                    result.Rows[i].Values.Add(double.Parse(coordinatesArray[k++]));
                }
            }

            return result;
        }
    }
}
