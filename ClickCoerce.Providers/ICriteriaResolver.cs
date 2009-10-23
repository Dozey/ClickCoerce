using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ClickCoerce.Providers
{
    internal interface ICriteriaResolver<TCriteria>
    {
        TCriteria GetCriteria(ICustomAttributeProvider attributeProvider);
        TCriteria GetCriteria(ICustomAttributeProvider attributeProvider, BindingMode mode);
        bool TryGetCriteria(ICustomAttributeProvider attributeProvider, out TCriteria criteria);
        bool TryGetCriteria(ICustomAttributeProvider attributeProvider, BindingMode mode, out TCriteria criteria);
    }
}
