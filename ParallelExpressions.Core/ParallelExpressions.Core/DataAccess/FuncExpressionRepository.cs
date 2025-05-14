using Npgsql;
using ParallelExpressions.Core.Models;
using ParallelExpressions.Core.Services;
using System.Text;

namespace ParallelExpressions.Core.DataAccess
{
    public class FuncExpressionRepository
    {
        private readonly IConfiguration _configuration;

        private string? _connectionString;

        public FuncExpressionRepository(IConfiguration configuration) {
            _configuration = configuration;
            _connectionString = _configuration["ParallelExpressions:ConnectionString"];
        }

        public void Create(FuncExpression expression) {
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            StringBuilder sb = new StringBuilder();
            sb.Append($"delete from parallelexpressions.expression;");
            sb.Append($"delete from parallelexpressions.edge;");
            sb.Append($"delete from parallelexpressions.expression_matrix_result;");
            InsertNode(expression, sb);

            using (var cmd = dataSource.CreateCommand(sb.ToString()))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void Reset() 
        {
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            StringBuilder sb = new StringBuilder();
            sb.Append($"update parallelexpressions.expression set status = 0;");
            sb.Append($"update parallelexpressions.expression_matrix_result set coordinates = '';");

            using (var cmd = dataSource.CreateCommand(sb.ToString()))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateNode(FuncExpression expression)
        {
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            StringBuilder sb = new StringBuilder(); 
            sb.Append($"update parallelexpressions.expression set status = {(int)expression.Status}, type = {(int)expression.Type} where node = {expression.Id};");

            using (var cmd = dataSource.CreateCommand(sb.ToString()))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateExpressionMatrixResult(int expressionId, string coordinates)
        {
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            StringBuilder sb = new StringBuilder();
            sb.Append($"update parallelexpressions.expression_matrix_result set coordinates = '{coordinates}' where node = {expressionId};");

            using (var cmd = dataSource.CreateCommand(sb.ToString()))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public Graph GetGraph()
        {
            return new Graph { Nodes = GetNodes(), Edges = GetEdges() };
        }

        public List<Edge> GetEdges()
        {
            var result = new List<Edge>();
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            var connection = dataSource.OpenConnection();
            string commandString = $"select * from parallelexpressions.edge";
            using var commmand = new NpgsqlCommand(commandString, connection);
            using var reader = commmand.ExecuteReader();

            while (reader.Read())
            {
                var start = reader.GetFieldValue<int>(0);
                var end = reader.GetFieldValue<int>(1);
                result.Add(new Edge { Start = start, End = end });
            }

            connection.Close();
            return result;
        }

        public List<Matrix> GetMatrices() 
        {
            var result = new List<Matrix>();
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            var connection = dataSource.OpenConnection();
            string commandString = $"select * from parallelexpressions.matrix";
            using var commmand = new NpgsqlCommand(commandString, connection);
            using var reader = commmand.ExecuteReader();

            while (reader.Read())
            {
                var rows = reader.GetFieldValue<int>(0);
                var columns = reader.GetFieldValue<int>(1);
                var coordinates = reader.GetFieldValue<string>(2);
                var operand = reader.GetFieldValue<string>(3);
                result.Add(new Matrix { Rows = rows, Columns = columns, Coordinates = coordinates, Operand = operand });
            }

            connection.Close();
            return result;
        }

        public Matrix GetDataMatrix(string operand)
        {
            Matrix result = null;
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            var connection = dataSource.OpenConnection();
            string commandString = $"select rows, columns, coordinates from parallelexpressions.matrix where operand = '{operand}'";
            using var commmand = new NpgsqlCommand(commandString, connection);
            using var reader = commmand.ExecuteReader();

            while (reader.Read())
            {
                result = new Matrix();
                var rows = reader.GetFieldValue<int>(0);
                var columns = reader.GetFieldValue<int>(1);
                var coordinates = reader.GetFieldValue<string>(2);
                result.Rows = rows;
                result.Columns = columns;
                result.Coordinates = coordinates;
            }

            connection.Close();
            return result;
        }

        public Matrix GetResultMatrix(string label)
        {
            int node = GetNode(label);

            if (node == 0)
            {
                return null;
            }

            var matrix = GetResultMatrix(node);
            return matrix;
        }

        private List<Node> GetNodes()
        {
            var result = new List<Node>();
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            var connection = dataSource.OpenConnection();
            string commandString = $"select * from parallelexpressions.expression order by node";
            using var commmand = new NpgsqlCommand(commandString, connection);
            using var reader = commmand.ExecuteReader();

            while (reader.Read())
            {
                var node = reader.GetFieldValue<int>(0);
                var status = reader.GetFieldValue<int>(1);
                var type = reader.GetFieldValue<int>(2);
                var label = reader.GetFieldValue<string>(3);
                result.Add(new Node { Id = node, Status = status, Type = type, Label = label });
            }

            connection.Close();
            return result;
        }

        private int GetNode(string label)
        {
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            var connection = dataSource.OpenConnection();
            string commandString = $"select node from parallelexpressions.expression where label = '{label}'";
            using var commmand = new NpgsqlCommand(commandString, connection);
            using var reader = commmand.ExecuteReader();

            int result = 0;

            while (reader.Read())
            {
                result = reader.GetFieldValue<int>(0);
            }

            connection.Close();
            return result;
        }

        private Matrix GetResultMatrix(int node)
        {
            Matrix result = null;
            using var dataSource = NpgsqlDataSource.Create(_connectionString);
            var connection = dataSource.OpenConnection();
            string commandString = $"select rows, columns, coordinates from parallelexpressions.expression_matrix_result where node = {node} and coordinates != ''";
            using var commmand = new NpgsqlCommand(commandString, connection);
            using var reader = commmand.ExecuteReader();

            while (reader.Read())
            {
                result = new Matrix();
                var rows = reader.GetFieldValue<int>(0);
                var columns = reader.GetFieldValue<int>(1);
                var coordinates = reader.GetFieldValue<string>(2);
                result.Rows = rows;
                result.Columns = columns;
                result.Coordinates = coordinates;
            }

            connection.Close();
            return result;
        }

        private void InsertNode(FuncExpression expression, StringBuilder sb)
        {
            if (expression == null)
            {
                return;
            }

            sb.Append($"insert into parallelexpressions.expression (node, status, type, label) values ({expression.Id}, {(int)expression.Status}, {(int)expression.Type}, '{expression.Label}');");
            sb.Append($"insert into parallelexpressions.expression_matrix_result (node, rows, columns) values ({expression.Id}, 10, 10);");

            if (expression.ExList == null)
            {
                return;
            }

            foreach (var item in expression.ExList)
            {
                sb.Append($"insert into parallelexpressions.edge values ({expression.Id}, {item.Id});");
                InsertNode(item, sb);
            }
        }
    }
}
