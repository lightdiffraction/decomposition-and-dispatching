using MathNet.Numerics.LinearAlgebra;
using Microsoft.AspNetCore.Mvc;
using ParallelExpressions.Core.DataAccess;
using ParallelExpressions.Core.Services;

namespace ParallelExpressions.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParallelExpressionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ParallelExpressionsController> _logger;

        public ParallelExpressionsController(ILogger<ParallelExpressionsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("number-expression")]
        public async Task<IActionResult> NumberExpression([FromBody] ExpressionRequest request)
        {
            //((a + b) / (d * e)) + (l + m)
            var exA = new FuncExpression(1, 46, typeof(double), "a");
            var exB = new FuncExpression(2, 54, typeof(double), "b");
            var exC = new FuncExpression(3, exA, exB, MapReduceFuncs.Add, typeof(double), typeof(double), "+"); //5sec +

            var exD = new FuncExpression(4, 15, typeof(double), "d");
            var exE = new FuncExpression(5, 2, typeof(double), "e");
            var exF = new FuncExpression(6, exD, exE, MapReduceFuncs.Multiply, typeof(double), typeof(double), "*"); //3 sec *

            var exG = new FuncExpression(7, exC, exF, MapReduceFuncs.Divide, typeof(double), typeof(double), "/"); //2sec /

            var exL = new FuncExpression(8, 20.0, typeof(double), "l");
            var exM = new FuncExpression(9, 10.0, typeof(double), "m");
            var exN = new FuncExpression(10, exL, exM, MapReduceFuncs.Add, typeof(double), typeof(double), "+"); //5sec +

            var exK = new FuncExpression(11, exG, exN, MapReduceFuncs.Add, typeof(double), typeof(double), "+"); //5sec +

            var repository = new FuncExpressionRepository(_configuration);
            repository.Create(exK);
            var decomposer = new Decomposer<double?>(exK, _configuration);
            await decomposer.Decompose();

            return this.Ok();
        }

        [HttpPost("matrix-expression")]
        public async Task<IActionResult> MatrixExpression([FromBody] ExpressionRequest request)
        {
            //((m1 + m2) + (m3 + m4)) * (m5 + m6)
            var matrix1 = MatrixCreator.CreateMatrix1();
            var matrix2 = MatrixCreator.CreateMatrix2();
            var matrix3 = MatrixCreator.CreateMatrix3();
            var matrix4 = MatrixCreator.CreateMatrix4();
            var matrix5 = MatrixCreator.CreateMatrix5();
            var matrix6 = MatrixCreator.CreateMatrix6();

            var exA = new FuncExpression(1, matrix1, typeof(Matrix<double>), "m1");
            var exB = new FuncExpression(2, matrix2, typeof(Matrix<double>), "m2");
            var exC = new FuncExpression(3, exA, exB, MatrixMapReduceFuncs.Add, typeof(Matrix<double>), typeof(Matrix<double>), "+"); //5sec +

            var exD = new FuncExpression(4, matrix3, typeof(Matrix<double>), "m3");
            var exE = new FuncExpression(5, matrix4, typeof(Matrix<double>), "m4");
            var exF = new FuncExpression(6, exD, exE, MatrixMapReduceFuncs.Add, typeof(Matrix<double>), typeof(Matrix<double>), "+"); //3 sec *

            var exG = new FuncExpression(7, exC, exF, MatrixMapReduceFuncs.Add, typeof(Matrix<double>), typeof(Matrix<double>), "+"); //2sec /

            var exL = new FuncExpression(8, matrix5, typeof(Matrix<double>), "m5");
            var exM = new FuncExpression(9, matrix6, typeof(Matrix<double>), "m6");
            var exN = new FuncExpression(10, exL, exM, MatrixMapReduceFuncs.Add, typeof(Matrix<double>), typeof(Matrix<double>), "+"); //5sec +

            var exK = new FuncExpression(11, exG, exN, MatrixMapReduceFuncs.Multiply, typeof(Matrix<double>), typeof(Matrix<double>), "*"); //5sec +

            var repository = new FuncExpressionRepository(_configuration);
            repository.Create(exK);

            var decomposer = new Decomposer<double?>(exK, _configuration);

            await decomposer.Decompose();

            return this.Ok();
        }

        [HttpPost("parse-number-expression")]
        public async Task<IActionResult> ParseNumberExpression([FromBody] ExpressionRequest request)
        {
            //((46 + 54) * (15 + 2)) * (10 + 20)
            var exA = new FuncExpression(1, 46.0, typeof(double), "a");
            var exB = new FuncExpression(2, 54.0, typeof(double), "b");
            var exC = new FuncExpression(3, 15.0, typeof(double), "c");
            var exD = new FuncExpression(4, 2.0, typeof(double), "d");
            var exE = new FuncExpression(5, 10.0, typeof(double), "e");
            var exF = new FuncExpression(6, 20.0, typeof(double), "f");
            var funcDictionary = new Dictionary<string, FuncExpression>
            {
                { "a", exA },
                { "b", exB },
                { "c", exC },
                { "d", exD },
                { "e", exE },
                { "f", exF }
            };
            var ex = new StringToExpression("((a+b)*(c+d))*(e+f)", funcDictionary, FuncType.Number).Parse();

            var repository = new FuncExpressionRepository(_configuration);
            repository.Create(ex);
            var decomposer = new Decomposer<double?>(ex, _configuration);
            await decomposer.Decompose();

            return this.Ok();
        }

        [HttpPost("parse-matrix-expression")]
        public async Task<IActionResult> ParseMatrixExpression([FromBody] ExpressionRequest request)
        {
            //((m1 + m2) * (m3 + m4)) * (m5 + m6)

            var repository = new FuncExpressionRepository(_configuration);
            var matrixService = new MatrixService(repository);
            var matrices = matrixService.Read();
            var funcDictionary = new Dictionary<string, FuncExpression>();
            int id = 1;

            foreach (var matrix in matrices)
            {
                var funcExpression = new FuncExpression(id++, matrix.Coordinates, typeof(Matrix<double>), "");
                funcDictionary.Add(matrix.Operand, funcExpression);
            }
            var ex = new StringToExpression("((a+b)*(c+d))*(e+f)", funcDictionary, FuncType.Matrix).Parse();

            
            repository.Create(ex);
            var decomposer = new Decomposer<double?>(ex, _configuration);
            await decomposer.Decompose();

            return this.Ok();
        }


        [HttpPost("parse-const-matrix-expression")]
        public async Task<IActionResult> ParseConstMatrixExpression([FromBody] ExpressionRequest request)
        {
            //((m1 + m2) * (m3 + m4)) * (m5 + m6)
            var matrix1 = MatrixCreator.CreateMatrix1();
            var matrix2 = MatrixCreator.CreateMatrix2();
            var matrix3 = MatrixCreator.CreateMatrix3();
            var matrix4 = MatrixCreator.CreateMatrix4();
            var matrix5 = MatrixCreator.CreateMatrix5();
            var matrix6 = MatrixCreator.CreateMatrix6();
            var exA = new FuncExpression(1, matrix1, typeof(Matrix<double>), "m1");
            var exB = new FuncExpression(2, matrix2, typeof(Matrix<double>), "m2");
            var exC = new FuncExpression(3, matrix3, typeof(Matrix<double>), "m3");
            var exD = new FuncExpression(4, matrix4, typeof(Matrix<double>), "m4");
            var exE = new FuncExpression(5, matrix5, typeof(Matrix<double>), "m5");
            var exF = new FuncExpression(6, matrix6, typeof(Matrix<double>), "m6");
            var funcDictionary = new Dictionary<string, FuncExpression>
            {
                { "a", exA },
                { "b", exB },
                { "c", exC },
                { "d", exD },
                { "e", exE },
                { "f", exF }
            };
            var ex = new StringToExpression("((a+b)*(c+d))*(e+f)", funcDictionary, FuncType.Matrix).Parse();

            var repository = new FuncExpressionRepository(_configuration);
            repository.Create(ex);
            var decomposer = new Decomposer<double?>(ex, _configuration);
            await decomposer.Decompose();

            return this.Ok();
        }

        [HttpGet("graph")]
        public async Task<IActionResult> GetGraph()
        {
            var repository = new FuncExpressionRepository(_configuration);
            var result = repository.GetGraph();
            return this.Ok(result);
        }

        [HttpGet("reset")]
        public async Task<IActionResult> Reset()
        {
            var repository = new FuncExpressionRepository(_configuration);
            repository.Reset();
            return this.Ok();
        }

        [HttpGet("node/{label}")]
        public async Task<IActionResult> Node(string label)
        {
            var repository = new FuncExpressionRepository(_configuration);
            var matrixService = new MatrixService(repository);
            var result = matrixService.GetMatrix(label);
            return this.Ok(result);
        }
    }
}
