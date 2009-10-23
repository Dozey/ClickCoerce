using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ClickCoerce.Providers
{
    internal class ExpressionCacheModifier<TElement> : ExpressionVisitor
    {
        private bool cacheEnabled;
        private IQueryable<TElement> cache;

        public ExpressionCacheModifier()
        {
            cacheEnabled = false;
            cache = null;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Type.IsSubclassOf(typeof(Query<TElement>)))
            {
                Query<TElement> value = (Query<TElement>)c.Value;

                cacheEnabled = value.CacheEnabled;

                if (cache != null && value.Cache == null && cacheEnabled)
                {
                    value.Cache = cache;
                }
                else
                {
                    cache = value.Cache;
                }
            }

            return c;
        }

        public bool GetCache(Expression expression, out IQueryable<TElement> resultCache)
        {
            cache = null;
            Visit(expression);
            resultCache = cache;

            return cacheEnabled;
        }

        public bool SetCache(Expression expression, IQueryable<TElement> resultCache)
        {
            cache = resultCache;
            Visit(expression);
            return cacheEnabled;
        }


    }
}
