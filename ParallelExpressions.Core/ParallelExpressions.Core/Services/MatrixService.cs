using ParallelExpressions.Core.DataAccess;
using ParallelExpressions.Core.Services.Mappers;
using ParallelExpressions.Core.Services.Models;

namespace ParallelExpressions.Core.Services
{
    public class MatrixService
    {
        private FuncExpressionRepository _repository;

        public MatrixService(FuncExpressionRepository repository) 
        {
            _repository = repository;
        }

        public List<Models.Matrix> Read()
        {
            var matrices = _repository.GetMatrices();
            var mapper = new MatrixMapper();
            var result = matrices.Select(m => mapper.Map(m)).ToList();
            return result;
        }

        public MatrixResponse GetMatrix(string label)
        {
            var dataMatrix = _repository.GetDataMatrix(label);

            if (dataMatrix != null)
            {
                var mapper = new MatrixResponseMapper();
                var result = mapper.Map(dataMatrix);
                return result;
            }

            var resultMatrix = _repository.GetResultMatrix(label);

            if (resultMatrix != null)
            {
                var mapper = new MatrixResponseMapper();
                var result = mapper.Map(resultMatrix);
                return result;
            }

            return null;
        }
    }
}
