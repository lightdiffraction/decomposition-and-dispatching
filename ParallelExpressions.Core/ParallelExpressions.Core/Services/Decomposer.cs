using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ParallelExpressions.Core.DataAccess;
using ParallelExpressions.Core.Services.Converters;

namespace ParallelExpressions.Core.Services
{
    public class Decomposer<T>
    {
        private readonly IConfiguration _configuration;
        private FuncExpressionRepository _repository;
        IHubCallerClients _clients;
        FuncExpression expression;
        event EventHandler FuncFinished;
        object _lock = new object();

        public Decomposer(FuncExpression expression, IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = new FuncExpressionRepository(_configuration);
            this.expression = expression;
            FuncFinished += Decomposer_FuncFinished;
        }
        public Decomposer(IHubCallerClients clients, FuncExpression expression, IConfiguration configuration)
        {
            _clients = clients; 
            _configuration = configuration;
            _repository = new FuncExpressionRepository(_configuration);
            this.expression = expression;
            FuncFinished += Decomposer_FuncFinished;
        }

        private void Decomposer_FuncFinished(object? sender, EventArgs e)
        {
            Decompose(expression);
        }

        public async Task<IActionResult> Decompose()
        {
            if (_clients != null)
            {
                _clients.All.SendAsync("Started");
            }

            Decompose(expression);

            return new OkResult();
        }

        private void Decompose(FuncExpression expression)
        {
            if (expression == null)
            {
                return;
            }

            if (expression.Data != null)
            {
                return;
            }
            
            foreach (var item in expression.ExList)
            {
                if (item.Data == null && item.Status == ExpressionStatus.NotStarted)
                {
                    Decompose(item);
                }
            }

            bool allDataNotNull = expression.ExList.All(x => x.Data != null);

            if (expression.Data == null && allDataNotNull && expression.Status == ExpressionStatus.NotStarted)
            {
                Task.Factory.StartNew(() =>
                {
                    expression.Status = ExpressionStatus.InProgress;
                    _repository.UpdateNode(expression);
                    
                    if (_clients != null)
                    {
                        _clients.All.SendAsync("InProgress", expression.Id);
                    }

                    expression.Data = expression.Reduce(expression.ExList);
                    expression.Status = ExpressionStatus.Done;
                    _repository.UpdateNode(expression);
                    string expressionResult = ExpressionResultToStringConverter.Convert(expression.Data);
                    _repository.UpdateExpressionMatrixResult(expression.Id, expressionResult);
                    FuncFinished(this, null);

                    if (_clients != null)
                    {
                        _clients.All.SendAsync("Done", expression.Id);
                    }
                });
            }
        }
    }
}
