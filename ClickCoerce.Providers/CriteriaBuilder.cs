using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClickCoerce.Providers
{
    internal class CriteriaBuilder<TCriteria> : SortedDictionary<TCriteria, object>, ICriteriaBuilder<TCriteria> where TCriteria : struct
    {
        public CriteriaBuilder() : base()
        {
            if (!typeof(TCriteria).IsEnum)
                throw new NotSupportedException();
        }

        public Canon<TCriteria>[] Criteria
        {
            get
            {
                return this.Select<KeyValuePair<TCriteria, object>, Canon<TCriteria>>(kv => new Canon<TCriteria>(kv.Key, kv.Value)).ToArray();
            }
        }
    }
}
