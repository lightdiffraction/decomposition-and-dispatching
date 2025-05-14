namespace ParallelExpressions.Core.Services.Models
{
    public class MatrixResponse
    {
        public List<MatrixRow> Rows { get; set; } = new List<MatrixRow>();
    }

    public class MatrixRow
    {
        public List<double> Values { get; set; } = new List<double>();
    }
}
