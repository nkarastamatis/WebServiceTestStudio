using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace WebServiceTestStudio.Model
{
    class ExpandoObjectConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return true;
            //return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == typeof(System.String) && value != null && value.GetType().Namespace != typeof(System.String).Namespace)
            {
                return value.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            // Add a DictionaryPropertyDescriptor for each dictionary entry.
            var dictionary = value as IDictionary<String, object>;
            var properties = new System.Collections.ArrayList();
            foreach (var e in dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(dictionary, e.Key));
                if (e.Value != null)
                    TypeDescriptorModifier.modifyType(e.Value.GetType());
            }

            PropertyDescriptor[] props =
                (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }
    }
}
