using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ClickCoerce.Providers
{
    internal class CriteriaResolver<T> : ICriteriaResolver<T> where T : struct
    {
        private Type criteriaType = typeof(T);

        public CriteriaResolver()
        {
            if (!criteriaType.IsEnum)
                throw new NotSupportedException("T must inherit from Enum");
        }

        public T GetCriteria(ICustomAttributeProvider attributeProvider)
        {
            foreach (BindingAttribute attribute in attributeProvider.GetCustomAttributes(typeof(BindingAttribute), false))
            {
                if (attribute.Criteria.GetType() == criteriaType)
                {
                    return (T)Enum.ToObject(criteriaType, attribute.Criteria);
                }
            }

            return default(T);
        }

        public T GetCriteria(ICustomAttributeProvider attributeProvider, BindingMode mode)
        {
            foreach (BindingAttribute attribute in attributeProvider.GetCustomAttributes(typeof(BindingAttribute), false))
            {
                if (attribute.Criteria is T && (attribute.Mode & mode) == mode)
                {
                    return (T)Enum.ToObject(criteriaType, attribute.Criteria);
                }
            }

            return default(T);
        }

        public bool TryGetCriteria(ICustomAttributeProvider attributeProvider, out T criteria)
        {
            foreach (BindingAttribute attribute in attributeProvider.GetCustomAttributes(typeof(BindingAttribute), false))
            {
                if (attribute.Criteria is T)
                {
                    criteria = (T)Enum.ToObject(criteriaType, attribute.Criteria);

                    return true;
                }
            }

            criteria = default(T);

            return false;
        }

        public bool TryGetCriteria(ICustomAttributeProvider attributeProvider, BindingMode mode, out T criteria)
        {
            foreach (BindingAttribute attribute in attributeProvider.GetCustomAttributes(typeof(BindingAttribute), false))
            {
                if (attribute.Criteria is T && (attribute.Mode & mode) == mode)
                {
                    criteria = (T)Enum.ToObject(criteriaType, attribute.Criteria);

                    return true;
                }
            }

            criteria = default(T);

            return false;
        }
    }
}
