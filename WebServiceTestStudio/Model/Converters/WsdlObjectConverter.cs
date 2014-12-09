using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace WebServiceTestStudio.Model
{
    class WsdlObjectConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == typeof(System.String) && value != null && value.GetType().Namespace != "System")
            {
                return value.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            // Get any existing properties on the object
            var p = base.GetProperties(context, value, attributes);          
            var propertyDescriptors = new PropertyDescriptor[p.Count];
            p.CopyTo(propertyDescriptors, 0);
            var props = new PropertyDescriptorCollection(propertyDescriptors);

            // Add a WsdlFieldDescriptor for each field.            
            foreach (FieldInfo fi in value.GetType().GetFields())
            {
                var prop = new WsdlFieldDescriptor(fi, attributes);
                props.Add(prop);

                // Make sure the global TypeDescriptor uses the correct descriptor for this type.
                TypeDescriptorModifier.modifyType(fi.FieldType);
            }

            if (props.Count > 0)
                return props;

            return base.GetProperties(context, value, attributes);
        }
    }
}
