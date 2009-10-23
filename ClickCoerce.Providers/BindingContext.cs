using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ClickCoerce.Providers
{
    internal class BindingContext<TElement, TCriteria>
    {
        private Dictionary<TCriteria, List<PropertyInfo>> criteriaMap = new Dictionary<TCriteria, List<PropertyInfo>>();

        public BindingContext(ICriteriaResolver<TCriteria> resolver)
        {
            InitializeCriteriaMap(resolver);
        }

        private void InitializeCriteriaMap(ICriteriaResolver<TCriteria> criteriaResolver)
        {
            Type elementType = typeof(TElement);

            PropertyInfo[] properties = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                TCriteria criteria;

                if (criteriaResolver.TryGetCriteria(property, BindingMode.Read, out criteria))
                {
                    if (!criteriaMap.ContainsKey(criteria))
                        criteriaMap.Add(criteria, new List<PropertyInfo>(1));

                    criteriaMap[criteria].Add(property);
                }
            }
        }

        public int SetValue(TElement instance, TCriteria criteria, object value)
        {
            int setCount = 0;

            if(criteriaMap.ContainsKey(criteria))
                foreach (PropertyInfo property in criteriaMap[criteria])
                {
                    property.SetValue(instance, value, null);
                    setCount++;
                }

            return setCount;
        }

        public TCriteria[] SupportedCriteria
        {
            get { return criteriaMap.Keys.ToArray(); }
        }
    }
}
