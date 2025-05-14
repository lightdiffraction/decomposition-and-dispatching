using Microsoft.AspNetCore.Mvc;


namespace ParallelExpressions.Core.Services
{
    public class CommonSubexpressionEliminator<T>
    {
        private FuncExpression _expression;

        private int _expressionCount;

        private event EventHandler _funcFinished;

        private List<FuncExpression> _leafList = new List<FuncExpression>();

        public CommonSubexpressionEliminator(FuncExpression expression, int expressionCount)
        {
            this._expression = expression;
            this._expressionCount = expressionCount;

            _funcFinished += CommonSubexpressionEliminator_FuncFinished;
        }

        private void CommonSubexpressionEliminator_FuncFinished(object? sender, EventArgs e)
        {
            CommonSubexpressionEliminate(_expression);
        }

        public async Task<IActionResult> CommonSubexpressionEliminate()
        {
            CommonSubexpressionEliminate(_expression);

            return new OkResult();
        }

        public FuncExpression CommonSubexpressionEliminate(FuncExpression expression)
        {
            var finder = new TreeHelper();
            _leafList = finder.FindLeafList(expression).Distinct().ToList();
            Eliminate(expression);
            return expression;
        }

        public FuncExpression Eliminate(FuncExpression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.Data != null)
            {
                return expression;
            }

            var parentLists = FindParentLists(_leafList);
            var intersection = new List<FuncExpression>();

            foreach (var parentList in parentLists)
            {
                var moreThanSingleParentList = parentList.Value.Where(i => i.Count > 1).ToList();
                
                if (moreThanSingleParentList.Count > 0)
                {
                    var i = MaximumIntersection(moreThanSingleParentList);
                    
                    if (i.Count > intersection.Count)
                    {
                        intersection = i;
                    }
                }
            }

            bool linkFound = false;

            if (intersection.Count == 0)
            {
                return expression;
            }

            var link = intersection[0];
            var sameList = intersection[0].ExList;

            foreach (var parent in intersection)
            {
                sameList = sameList.Intersect(parent.ExList).ToList();
            }

            foreach (var parent in intersection)
            {
                if (Enumerable.SequenceEqual(parent.ExList, sameList) & !linkFound)
                {
                    link = parent;
                    linkFound = true;
                }
                else
                {
                    parent.ExList = parent.ExList.Except(sameList).ToList();
                    
                    foreach (var child in sameList)
                    {
                        child.Parents.Remove(parent);
                    }
                }
            }

            if (linkFound)
            {
                foreach (var parent in intersection)
                {
                    if (link != parent)
                    {
                        if (parent.ExList.Count == 0)
                        {
                            foreach (var grandmother in parent.Parents)
                            {
                                grandmother.ExList.Remove(parent);
                                grandmother.ExList.Add(link);
                                link.Parents.Add(grandmother);
                            }
                        }
                        else
                        {
                            parent.ExList.Add(link);
                            link.Parents.Add(parent);
                        }
                    }
                }
            }
            else
            {
                link = new FuncExpression(_expressionCount++, sameList,
                        intersection[0].Reduce, intersection[0].InputType, intersection[0].OutputType, "");

                foreach (var same in sameList)
                {
                    same.Parents.Add(link);
                }

                foreach (var parent in intersection)
                {
                    parent.ExList.Add(link);
                    link.Parents.Add(parent);
                }
            }

            _leafList.Add(link);
            Eliminate(expression);
            return expression;
        }

        public Dictionary<Func<List<FuncExpression>, object>, List<List<FuncExpression>>> FindParentLists (List<FuncExpression> LeaveList)
        {
            var dict = new Dictionary <Func<List<FuncExpression>, object>,  List <List<FuncExpression>>> ();
            for (int i = 0; i < LeaveList.Count; i++)
            {
                if (LeaveList[i].Parents.Count > 1)
                {
                    foreach (var parent in LeaveList[i].Parents)
                    {
                        if (!dict.ContainsKey(parent.Reduce))
                        {
                            dict.Add(parent.Reduce, new List<List<FuncExpression>> ());
                        }
                        try
                        {
                            if (dict[parent.Reduce].Count == 0)
                            {
                                for (int j = 0; j < LeaveList.Count; j++)
                                {
                                    dict[parent.Reduce].Add(new List<FuncExpression>());
                                }
                            }
                            dict[parent.Reduce][i].Add(parent);
                        }
                        catch (Exception e)
                        { 
                        }
                    }
                }
            }
            return dict;
        }

        public List<FuncExpression> MaximumIntersection(List<List<FuncExpression>> InitialLists)
        {
            var resultList = new List<WeightedFuncExpressionList<T>>();
            foreach (var item in InitialLists)
            {
                int weight = 1;
                bool Add = true;
                if (resultList.Count == 0)
                {
                    resultList.Add(new WeightedFuncExpressionList<T>(item, weight));
                }
                else
                {
                    foreach (var compare in resultList)
                    {
                        bool ItemIncludesCompare = (compare.ExpressionList.Distinct().Except(item.Distinct()).ToList().Count == 0);
                        bool CompareIncludesItem = (item.Distinct().Except(compare.ExpressionList.Distinct()).ToList().Count == 0);
                        if (CompareIncludesItem) {
                            weight++;
                        }
                        if (ItemIncludesCompare)
                        {
                            compare.Weight = compare.Weight + 1;
                        }
                        if (CompareIncludesItem & ItemIncludesCompare)
                        {
                            Add = false;
                        }
                    }
                    if (Add)
                    {
                        resultList.Add(new WeightedFuncExpressionList<T>(item, weight));
                    }
                }
            }

            var maxWeight = resultList.Max(i => i.Weight);
            if (maxWeight <= 1)
            {
                return new List<FuncExpression>();
            }
            var maxWeightItems = resultList.Where(i => i.Weight == maxWeight);
            var maxExpressionListCount = maxWeightItems.Max(i => i.ExpressionList.Count);
            var maxExpressionListCountItems = maxWeightItems.Where(i => i.ExpressionList.Count == maxExpressionListCount);
            return maxExpressionListCountItems.First().ExpressionList;
        }
    }
    public class WeightedFuncExpressionList<T>
    {
        public List<FuncExpression> ExpressionList { get; set; }
        public int Weight { get; set; }

        public WeightedFuncExpressionList (List<FuncExpression> expressionList, int weight)
        {
            ExpressionList = expressionList;
            Weight = weight;
        }
    }
}
