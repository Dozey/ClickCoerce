using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace ClickCoerce.Providers
{
    internal abstract class BaseExpressionCriteriaVistitor<TElement, TCriteria> : ExpressionVisitor
    {
        public BaseExpressionCriteriaVistitor(ICriteriaBuilder<TCriteria> criteriaBuilder, ICriteriaResolver<TCriteria> criteriaResolver)       
        {
            if (criteriaBuilder == null)
                throw new ArgumentNullException("criteriaBuilder");

            if (criteriaResolver == null)
                throw new ArgumentNullException("criteriaResolver");

            CriteriaBuilder = criteriaBuilder;
            CriteriaResolver = criteriaResolver;
        }

        public ICriteriaBuilder<TCriteria> CriteriaBuilder { get; protected set; }
        public ICriteriaResolver<TCriteria> CriteriaResolver { get; protected set; }

        public Canon<TCriteria>[] GetCriteria(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            CriteriaBuilder.Clear();
            Visit(expression);

            return CriteriaBuilder.Criteria;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    return VisitEqual(b);
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return VisitGreater(b);
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return VisitLess(b);
            }

            return base.VisitBinary(b);
        }

        protected virtual Expression VisitEqual(BinaryExpression expression)
        {
            MemberInfo member;
            object value;

            if (GetBinaryParticipants(expression, out member, out value))
            {
                if (!CheckTypeConstraint(member))
                    throw new ArgumentException("expression");

                TCriteria criteria;

                if (CriteriaResolver.TryGetCriteria(member, BindingMode.Read, out criteria))
                {
                    if(!CriteriaBuilder.ContainsKey(criteria))
                    {
                        CriteriaBuilder.Add(criteria, value);
                    }
                }
            }

            return expression;
        }

        protected virtual Expression VisitGreater(BinaryExpression expression)
        {
            MemberInfo member;
            object value;

            if (GetBinaryParticipants(expression, out member, out value))
            {
                if (!CheckTypeConstraint(member))
                    throw new ArgumentException("expression");
            }

            return expression;
        }

        protected virtual Expression VisitLess(BinaryExpression expression)
        {
            MemberInfo member;
            object value;

            if (GetBinaryParticipants(expression, out member, out value))
            {
                if (!CheckTypeConstraint(member))
                    throw new ArgumentException("expression");
            }

            return expression;
        }

        protected bool GetBinaryParticipants(BinaryExpression b, out MemberInfo member, out object value)
        {
            member = null;
            value = null;

            if (b.Left.NodeType == ExpressionType.MemberAccess && b.Right.NodeType == ExpressionType.Constant)
            {
                member = ((MemberExpression)b.Left).Member;
                value = ((ConstantExpression)b.Right).Value;
            }

            if (b.Right.NodeType == ExpressionType.MemberAccess && b.Left.NodeType == ExpressionType.Constant)
            {
                member = ((MemberExpression)b.Right).Member;
                value = ((ConstantExpression)b.Left).Value;
            }

            if (member != null && value != null)
                return true;
            else
                return false;
        }

        protected bool CheckTypeConstraint(MemberInfo member)
        {
            return (member.DeclaringType == typeof(TElement));
        }
    }
}
