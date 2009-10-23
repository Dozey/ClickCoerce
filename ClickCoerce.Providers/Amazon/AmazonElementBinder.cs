using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClickCoerce.Providers;
using Amazon.ECS;
using Amazon.ECS.Query;
using Amazon.ECS.Model;

namespace ClickCoerce.Providers.Amazon
{
    internal class AmazonElementBinder<TElement>
    {

        private BindingContext<TElement, AmazonCriteria> bindingContext;

        public AmazonElementBinder(BindingContext<TElement, AmazonCriteria> bindingContext)
        {
            this.bindingContext = bindingContext;
        }

        public TElement Bind(TElement element, Item item)
        {
            foreach (AmazonCriteria criteria in bindingContext.SupportedCriteria)
            {
                switch (criteria)
                {
                    case AmazonCriteria.Manufacturer:
                        bindingContext.SetValue(element, criteria, item.ItemAttributes.Manufacturer);
                        break;
                    case AmazonCriteria.Model:
                        bindingContext.SetValue(element, criteria, item.ItemAttributes.Model);
                        break;
                    case AmazonCriteria.Title:
                        bindingContext.SetValue(element, criteria, item.ItemAttributes.Title);
                        break;
                    case AmazonCriteria.ProductGroup:
                        bindingContext.SetValue(element, criteria, item.ItemAttributes.ProductGroup);
                        break;
                }
            }

            return element;
        }
    }
}
