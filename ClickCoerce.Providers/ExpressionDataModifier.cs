using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ClickCoerce.Providers
{
    internal class ExpressionDataModifier<TElement> : ExpressionVisitor
    {
        private IQueryable<TElement> data;

        public ExpressionDataModifier(IQueryable<TElement> dataSource)
        {
            data = dataSource;
        }

        public Expression CopyAndModify(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Type.IsSubclassOf(typeof(Query<TElement>)))
            {
                Query<TElement> value = (Query<TElement>)c.Value;
                return Expression.Constant(data);
            }
            else
            {
                return c;
            }
        }
    }
}
