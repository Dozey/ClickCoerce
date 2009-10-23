using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using ClickCoerce.Providers;

namespace ClickCoerce.Providers.Amazon
{
    internal class AmazonProvider : IQueryProvider
    {
        public AmazonProvider()
        {
            Context = new AmazonContext();
        }

        public AmazonContext Context { get; private set; }

        #region IQueryProvider Members

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new AmazonQuery<TElement>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);

            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(AmazonQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public TElement Execute<TElement>(Expression expression)
        {
            bool isEnumerable = (typeof(TElement).GetGenericTypeDefinition() == typeof(IEnumerable<>) && typeof(TElement).GetGenericArguments().Length == 1);

            if (isEnumerable)
            {
                try
                {
                    Type elementType = TypeSystem.GetElementType(expression.Type);
                    return (TElement)typeof(AmazonContext).GetMethod("Execute", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(elementType).Invoke(Context, new object[] { expression, isEnumerable });

                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            else
            {
                return (TElement)Context.Execute<TElement>(expression, isEnumerable);
            }
        }

        public object Execute(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);

            try
            {
                return typeof(AmazonContext).GetMethod("Execute", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(elementType).Invoke(Context, new object[] { expression, false });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        #endregion
    }
}
