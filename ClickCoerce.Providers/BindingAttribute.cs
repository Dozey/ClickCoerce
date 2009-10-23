using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClickCoerce.Providers
{
    public enum BindingMode { Read, Write }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
    public class BindingAttribute : Attribute
    {
        public BindingAttribute(object criteria)
        {
            Criteria = (Enum)criteria;
            Mode = BindingMode.Write;
        }

        public Enum Criteria { get; private set; }
        public BindingMode Mode { get; set; }
    }
}
