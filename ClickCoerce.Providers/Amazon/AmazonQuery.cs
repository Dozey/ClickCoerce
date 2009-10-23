using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ClickCoerce.Providers;

namespace ClickCoerce.Providers.Amazon
{
    public class AmazonQuery<T> : Query<T>
    {
        //public static AmazonQuery<T> Electronics
        //{
        //    get
        //    {
        //        AmazonQuery<T> query = new AmazonQuery<T>();
        //        // add product - contextual query here perahps
        //        query.Context.SearchIndex = AmazonSerchIndexes.Electronics;

        //        return query;
        //    }
        //}

        public AmazonQuery()
        {
            Provider = new AmazonProvider();
            Expression = Expression.Constant(this);
        }

        public AmazonQuery(bool cacheEnabled) : this()
        {
            CacheEnabled = cacheEnabled;
        }

        public AmazonQuery(IQueryProvider provider, Expression expression)
            : this(provider, expression, false)
        {
        }

        public AmazonQuery(IQueryProvider provider, Expression expression, bool cacheEnabled)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (expression == null)
                throw new ArgumentNullException("expression");

            Provider = provider;
            Expression = expression;
            CacheEnabled = cacheEnabled;
        }
    }
}
