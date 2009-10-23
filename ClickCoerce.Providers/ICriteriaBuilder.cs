using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClickCoerce.Providers
{
    internal interface ICriteriaBuilder<TCriteria> : IEnumerable<KeyValuePair<TCriteria, object>>
    {
        void Add(TCriteria criteria, object value);
        bool ContainsKey(TCriteria key);
        void Clear();
        bool Remove(TCriteria key);
        Canon<TCriteria>[] Criteria { get; }
    }
}
