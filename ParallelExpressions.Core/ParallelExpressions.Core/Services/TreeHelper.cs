namespace ParallelExpressions.Core.Services
{
    public class TreeHelper
    {
        private List<FuncExpression> _leafList = [];

        public void FindLeafListRecursive(FuncExpression expression)
        {
            if (expression == null)
            {
                return;
            }

            if (expression.Type == ExpressionType.Data)
            {
                _leafList.Add(expression);
                return;
            }

            foreach (var item in expression.ExList)
            {
                FindLeafListRecursive(item);
            }
        }

        public List<FuncExpression> FindLeafList(FuncExpression expression)
        {
            FindLeafListRecursive(expression);
            return _leafList;
        }

    }
}
