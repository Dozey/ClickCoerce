using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ClickCoerce.Providers;

namespace ClickCoerce.Providers.Amazon
{
    internal class AmazonExpressionCriteriaVisitor<TElement> : BaseExpressionCriteriaVistitor<TElement, AmazonCriteria>
    {
        public AmazonExpressionCriteriaVisitor(ICriteriaBuilder<AmazonCriteria> criteriaBuilder, ICriteriaResolver<AmazonCriteria> criteriaResolver)
            : base(criteriaBuilder, criteriaResolver)
        {
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return b;
            }

            return base.VisitBinary(b);
        }
    }
}
