using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using CodeBinding;

namespace CodeBinding.Rx
{
    public static class ObservableEx
    {

        static object s1, s2;

        public static IObservable<TResult> FromExpression<TResult>(Expression<Func<TResult>> expression)
        {
            BindingTarget<TResult> obj = new BindingTarget<TResult>();
            Expression<Func<bool>> expr = () => obj.Value == null;
            var visitor = new ObservableExVisitor(expression.Body);
            expr = (Expression<Func<bool>>)visitor.Visit(expr);
            System.Windows.Data.BindingBase binding = BindingEx.FromExpression(expr);
            s1 = obj;
            s2 = binding;
            return obj.ToObservable();
        }
    }

    internal class ObservableExVisitor : ExpressionVisitor
    {
        private Expression m_Expression;

        public ObservableExVisitor(Expression expression)
        {
            m_Expression = expression;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);

            return Expression.Equal(left, right);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return (node.Value == null) ? m_Expression : base.VisitConstant(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return (node.NodeType == ExpressionType.Convert) ? node.Operand : base.VisitUnary(node);
        }
    }
}
