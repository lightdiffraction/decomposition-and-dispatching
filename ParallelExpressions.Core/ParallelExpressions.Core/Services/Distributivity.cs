namespace ParallelExpressions.Core.Services
{
    public static class Distributivity
    {
        public static Dictionary<Tuple<Func<List<FuncExpression>, object>, Func<List<FuncExpression>, object>>, bool> Distributive =
            new Dictionary<Tuple<Func<List<FuncExpression>, object>, Func<List<FuncExpression>, object>>, bool>()
            {
                { new Tuple<Func<List<FuncExpression>, object>, Func<List<FuncExpression>, object>>(MatrixMapReduceFuncs.Add, MatrixMapReduceFuncs.Multiply), false},
                { new Tuple<Func<List<FuncExpression>, object>, Func<List<FuncExpression>, object>>(MatrixMapReduceFuncs.Multiply, MatrixMapReduceFuncs.Add), true},
            };
    }
}
