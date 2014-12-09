using System;
using System.ComponentModel;
using System.Globalization;

namespace WebServiceTestStudio.Model
{
    class ArrayConverter : ExpandableObjectConverter
    {
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
            // Add a ElementDescriptor for each items in the collection.
            var props = new PropertyDescriptorCollection(null);

            if (value is Array)
            {
                var collection = (Array)value;
                Type arrayType = collection.GetType().GetElementType();
                for (int i = 0; i < collection.Length; i++)
                {
                    var prop = new ElementDescriptor(arrayType, collection, i);
                    props.Add(prop);
                }
            }

            if (props.Count > 0)
                return props;

            return base.GetProperties(context, value, attributes);
        }
    }
}
