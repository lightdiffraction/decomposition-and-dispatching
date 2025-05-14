using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ParallelExpressions.Core.DataAccess;

namespace ParallelExpressions.Core.Services
{
    public class Transformator <T>
    {
        private FuncExpression expression;
        
        private int _expressionCount;

        private event EventHandler FuncFinished;

        public Transformator(FuncExpression expression, int expressionCount)
        {
            this.expression = expression;
            this._expressionCount = expressionCount;

            FuncFinished += Transformator_FuncFinished;
        }

        private void Transformator_FuncFinished(object? sender, EventArgs e)
        {
            Transform(expression);
        }

        public async Task<IActionResult> Transform()
        {
            Transform(expression);

            return new OkResult();
        }

        public FuncExpression Transform(FuncExpression expression)
        {
            if (expression == null)
            {
                return null;
            }
            if (expression.Data != null)
            {  
                return expression; 
            }

            if (expression.ExList.Count <= 1)
            {
                return expression;
            }
            else
            {
                bool inapplicable = false;
                var innerReduce = expression.ExList[0].Reduce;
                var DistributivityCompare = new Tuple<Func<List<FuncExpression>, object>, Func<List<FuncExpression>, object>>
                    (innerReduce, expression.Reduce);
                var sameList = expression.ExList[0].ExList;
                if (Distributivity.Distributive.ContainsKey(DistributivityCompare))
                {
                    if (Distributivity.Distributive[DistributivityCompare])
                    {
                        foreach (var ex in expression.ExList)
                        {
                            Transform(ex);
                            
                            if (ex.Reduce != innerReduce)
                            {
                                inapplicable = true;
                                break;
                            }
                            else
                            {
                                sameList = sameList.Intersect(ex.ExList).ToList();
                            }
                        }
                    }
                    else
                    {
                        foreach (var ex in expression.ExList)
                    {
                        Transform(ex);
                    }
                    return expression;
                    }
                }
                else
                {
                    foreach (var ex in expression.ExList)
                    {
                        Transform(ex);
                    }
                    return expression;
                }
                if (!inapplicable & sameList.Count != 0)
                {

                    foreach (var same in sameList)
                    {
                        same.Parents = same.Parents.Except(expression.ExList).ToList();
                    }

                    var expCopy = new FuncExpression(_expressionCount++, expression.ExList, 
                        expression.Reduce, expression.InputType, expression.OutputType, "");

                    foreach (var ex in expression.ExList)
                    {
                        ex.Parents.Remove(expression);
                        ex.Parents.Add(expCopy);
                        foreach (var same in sameList)
                        {
                            ex.ExList.Remove(same);
                        }
                    }

                    for (int i = 0; i < expCopy.ExList.Count; i++)
                    {
                        if (expCopy.ExList[i].ExList != null)
                        {
                            if (expCopy.ExList[i].ExList.Count == 1)
                            {
                                expCopy.ExList[i].ExList[0].Parents.Remove(expCopy.ExList[i]);
                                expCopy.ExList[i].ExList[0].Parents.Add(expCopy);
                                expCopy.ExList[i] = expCopy.ExList[i].ExList[0];
                            }
                        }
                    }
                    foreach (var same in sameList)
                    {
                        same.Parents.Add(expression);
                    }
                    sameList.Add(expCopy);
                    expCopy.Parents.Add(expression);
                    expression.Reduce = innerReduce;
                    expression.ExList = sameList;
                    

                    for (int i = 0; i < expression.ExList.Count; i++)
                    {
                        if (expression.ExList[i].ExList != null && expression.ExList[i].ExList.Count == 1)
                        {
                            expression.ExList[i].ExList[0].Parents.Remove(expression.ExList[i]);
                            expression.ExList[i].ExList[0].Parents.Add(expression);
                            expression.ExList[i] = expression.ExList[i].ExList[0];
                        }
                    }
                }

                return expression;
            }
        }
    }
}
