using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ClickCoerce.Providers
{
    public abstract class Query<T> : IQueryable<T>
    {
        public virtual bool CacheEnabled { get; set; }
        internal virtual IQueryable<T> Cache { get; set; }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        #endregion

        #region IQueryable Members

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression
        {
            get;
            protected set;
        }

        public IQueryProvider Provider
        {
            get;
            protected set;
        }

        #endregion
    }
}
