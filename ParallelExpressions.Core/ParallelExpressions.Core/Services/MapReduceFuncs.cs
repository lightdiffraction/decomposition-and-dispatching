namespace ParallelExpressions.Core.Services
{
    public static class MapReduceFuncs
    {
        public static Func<List<FuncExpression>, object> Add = (ex) =>
        {
            Thread.Sleep(5000);

            if (ex.Count == 0)
            {
                return null;
            }

            if (ex[0].InputType == typeof(double) && ex[0].OutputType == typeof(double))
            {
                double? result = 0;
                foreach (FuncExpression item in ex)
                {
                    result += (double)item.Data;
                }
                return result;
            }

            return null;
        };

        public static Func<List<FuncExpression>, object> Multiply = (ex) =>
        {
            Thread.Sleep(3000);

            if (ex.Count == 0)
            {
                return null;
            }

            if (ex[0].OutputType == typeof(double))
            {
                double? result = 1;
                foreach (FuncExpression item in ex)
                {
                    result *= (double)item.Data;
                }
                return result;
            }

            return null;
        };

        public static Func<List<FuncExpression>, object> Divide = (ex) =>   
        {
            Thread.Sleep(2000);

            if (ex.Count == 0)
            {
                return null;
            }

            if (ex[0].InputType == typeof(double) && ex[0].OutputType == typeof(double))
            {
                double? result = (double)ex[0].Data;
                for (int i = 1; i < ex.Count; i++)
                {
                    result /= (double)ex[i].Data;
                }
                return result;
            }

            return null;
        };
    }
}
