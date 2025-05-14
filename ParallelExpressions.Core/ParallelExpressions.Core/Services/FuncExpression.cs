using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ParallelExpressions.Core.Services
{
    public class FuncExpression
    {
        private string _label;

        public int Id { get; set; }

        public string Label 
        { 
            get
            {
                if (this.Type == ExpressionType.Data)
                {
                    return this._label;
                }

                var exListLabels = ExList.Select(e => e.Type == ExpressionType.Data ? e.Label : string.Format($"({e.Label})"));
                return string.Join(_label, exListLabels);
            }
        }

        public ExpressionType Type { get; }

        public object Data { get; set; }

        public Type OutputType { get; set; }

        public Type InputType { get; set; }

        public List<FuncExpression> ExList { get; set;}

        public Func<List<FuncExpression>, object> Reduce { get; set; }

        public ExpressionStatus Status { get; set; }

        public List<FuncExpression> Parents { get; set; } = new List<FuncExpression>();

        public FuncExpression(int id, object data, Type type, string label)
        {
            this.Id = id;
            this.Data = data;
            this.Type = ExpressionType.Data;
            this._label = label;
            this.InputType = type;
            this.OutputType = type;
            this.Status = ExpressionStatus.NotStarted;
        }

        public FuncExpression(int id, List<FuncExpression> exList, Func<List<FuncExpression>, object> reduce, Type inputType, Type outputType, string label)
        {
            this.Id = id;
            this.Type = ExpressionType.Operation;
            this.ExList = exList;
            this.Reduce = reduce;
            this.OutputType = outputType;
            this.InputType = inputType;
            this._label = label;
            this.Status = ExpressionStatus.NotStarted;
        }

        public FuncExpression(int id, FuncExpression ex1, FuncExpression ex2, Func<List<FuncExpression>, object> reduce, Type inputType, Type outputType, string label)
        {
            this.Id = id;
            this.Type = ExpressionType.Operation;
            this.ExList = [ex1, ex2];
            this.Reduce = reduce;
            this.OutputType = outputType;
            this.InputType = inputType;
            this._label = label;
            this.Status = ExpressionStatus.NotStarted;
        }

        public FuncExpression(FuncExpression ex1, FuncExpression ex2, Func<List<FuncExpression>, object> reduce, Type inputType, Type outputType, string label)
        {
            this.Type = ExpressionType.Operation;
            this.ExList = [ex1, ex2];
            this.Reduce = reduce;
            this.OutputType = outputType;
            this.InputType = inputType;
            this._label = label;
            this.Status = ExpressionStatus.NotStarted;
        }
    }
}
