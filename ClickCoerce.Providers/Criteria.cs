using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClickCoerce.Providers
{
    internal class Canon<TCriteria>
    {
        public Canon(TCriteria criteria, object value)
        {
            Criteria = criteria;
            Value = value;
        }

        public TCriteria Criteria { get; private set; }
        public object Value { get; private set; }
    }
}
