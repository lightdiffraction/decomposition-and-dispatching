using System.Text;

namespace ParallelExpressions.Core.Services
{
    public class StringToExpression
    {
        private string[] _funcOperators;

        private string[] _numberFuncOperators = { "+", "/", "*" };

        private string[] _matrixFuncOperators = { "+", "*" };

        private Func<FuncExpression, FuncExpression, FuncExpression>[] _funcOperations;

        private Func<FuncExpression, FuncExpression, FuncExpression>[] _numberFuncOperations =
        {
            (a1, a2) => new FuncExpression(a1, a2, MapReduceFuncs.Add, typeof(double), typeof(double), "+"),
            (a1, a2) => new FuncExpression(a1, a2, MapReduceFuncs.Divide, typeof(double), typeof(double), "/"),
            (a1, a2) => new FuncExpression(a1, a2, MapReduceFuncs.Multiply, typeof(double), typeof(double), "*"),
        };

        private Func<FuncExpression, FuncExpression, FuncExpression>[] _matrixFuncOperations =
{
            (a1, a2) => new FuncExpression(a1, a2, MatrixMapReduceFuncs.Add, typeof(double), typeof(double), "+"),
            (a1, a2) => new FuncExpression(a1, a2, MatrixMapReduceFuncs.Multiply, typeof(double), typeof(double), "*"),
        };

        private int _currentExpressionId;

        private string _textExpression;

        private Dictionary<string, FuncExpression> _funcs;

        public StringToExpression(string textExpression, Dictionary<string, FuncExpression> funcs, FuncType funcType)
        {
            this._textExpression = textExpression;
            this._funcs = funcs;
            this._currentExpressionId = 1;

            if (funcType == FuncType.Number)
            {
                this._funcOperators = this._numberFuncOperators;
                this._funcOperations = this._numberFuncOperations;
            }
            else if (funcType == FuncType.Matrix)
            {
                this._funcOperators = this._matrixFuncOperators;
                this._funcOperations = this._matrixFuncOperations;
            }
        }

        public int ExpressionsCount { get { return _currentExpressionId; } }

        public FuncExpression Parse()
        {
            return this.Parse(this._textExpression, this._funcs);
        }

        public FuncExpression Parse(string textExpression, Dictionary<string, FuncExpression> funcs)
        {
            List<string> tokens = getTokens(textExpression);
            Stack<FuncExpression> operandStack = new Stack<FuncExpression>();
            Stack<string> operatorStack = new Stack<string>();
            int tokenIndex = 0;

            while (tokenIndex < tokens.Count)
            {
                string token = tokens[tokenIndex];
                if (token == "(")
                {
                    string subExpr = getSubExpression(tokens, ref tokenIndex);
                    operandStack.Push(Parse(subExpr, funcs));
                    continue;
                }
                if (token == ")")
                {
                    throw new ArgumentException("Mis-matched parentheses in expression");
                }
                //If this is an operator  
                if (Array.IndexOf(_funcOperators, token) >= 0)
                {
                    while (operatorStack.Count > 0 && Array.IndexOf(_funcOperators, token) < Array.IndexOf(_funcOperators, operatorStack.Peek()))
                    {
                        string op = operatorStack.Pop();
                        FuncExpression arg2 = operandStack.Pop();
                        FuncExpression arg1 = operandStack.Pop();
                        var func = _funcOperations[Array.IndexOf(_funcOperators, op)](arg1, arg2);
                        arg2.Parents.Add(func);
                        arg1.Parents.Add(func);
                        func.Id = _currentExpressionId++;
                        operandStack.Push(func);
                    }
                    operatorStack.Push(token);
                }
                else
                {
                    funcs[token].Id = _currentExpressionId++;
                    operandStack.Push(funcs[token]);
                }
                tokenIndex += 1;
            }

            while (operatorStack.Count > 0)
            {
                string op = operatorStack.Pop();
                FuncExpression arg2 = operandStack.Pop();
                FuncExpression arg1 = operandStack.Pop();
                var func = _funcOperations[Array.IndexOf(_funcOperators, op)](arg1, arg2);
                arg2.Parents.Add(func);
                arg1.Parents.Add(func);
                func.Id = _currentExpressionId++;
                operandStack.Push(func);
            }
            return operandStack.Pop();
        }

        private string getSubExpression(List<string> tokens, ref int index)
        {
            StringBuilder subExpr = new StringBuilder();
            int parenlevels = 1;
            index += 1;
            while (index < tokens.Count && parenlevels > 0)
            {
                string token = tokens[index];
                if (tokens[index] == "(")
                {
                    parenlevels += 1;
                }

                if (tokens[index] == ")")
                {
                    parenlevels -= 1;
                }

                if (parenlevels > 0)
                {
                    subExpr.Append(token);
                }

                index += 1;
            }

            if ((parenlevels > 0))
            {
                throw new ArgumentException("Mis-matched parentheses in expression");
            }
            return subExpr.ToString();
        }

        private List<string> getTokens(string expression)
        {
            string operators = "()^*/+-";
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (char c in expression.Replace(" ", string.Empty))
            {
                if (operators.IndexOf(c) >= 0)
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    sb.Append(c);
                }
            }

            if ((sb.Length > 0))
            {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }
    }
}
