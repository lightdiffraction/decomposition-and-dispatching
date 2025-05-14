using MathNet.Numerics.LinearAlgebra;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using ParallelExpressions.Core.DataAccess;
using ParallelExpressions.Core.Services;

namespace ParallelExpressions.Core.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IConfiguration _configuration;

        public ChatHub(IConfiguration configuration) 
        {
            _configuration = configuration;
        }
        
        public async Task StartCalculation(string expression)
        {
            var repository = new FuncExpressionRepository(_configuration);
            var matrixService = new MatrixService(repository);
            var matrices = matrixService.Read();
            var funcDictionary = new Dictionary<string, FuncExpression>();
            int id = 1;

            foreach (var matrix in matrices)
            {
                var funcExpression = new FuncExpression(id++, matrix.Coordinates, typeof(Matrix<double>), matrix.Operand);
                funcDictionary.Add(matrix.Operand, funcExpression);
            }

            var stringToExpression = new StringToExpression(expression, funcDictionary, FuncType.Matrix);
            var ex = stringToExpression.Parse();
            //var transformator = new Transformator<double?>(ex, stringToExpression.ExpressionsCount);
            //transformator.Transform(ex);
            var eliminator = new CommonSubexpressionEliminator<double?>(ex, stringToExpression.ExpressionsCount);
            eliminator.CommonSubexpressionEliminate(ex);
            repository.Create(ex);
            var decomposer = new Decomposer<double?>(Clients, ex, _configuration);
            await decomposer.Decompose();
        }
    }
}
