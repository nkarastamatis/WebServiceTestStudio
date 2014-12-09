using System;
using System.ComponentModel;

namespace WebServiceTestStudio.Model
{
    // Helper class to assign the appropriate type converter based on type
    // the the property grid can correctly display the WSDL objects.
    public class TypeDescriptorModifier
    {
        public static void modifyType(Type type)
        {
            if (type.FullName == "System.Dynamic.ExpandoObject")
            {
                TypeDescriptor.AddAttributes(type, new TypeConverterAttribute(typeof(ExpandoObjectConverter)));
            }
            else if (type.BaseType.FullName == "System.Array")
            {
                TypeDescriptor.AddAttributes(type, new TypeConverterAttribute(typeof(ArrayConverter)));
                Type elemType = type.GetElementType();
                TypeDescriptorModifier.modifyType(elemType);
            }
            else if (type.Namespace != "System" && type.BaseType.Name != "Enum")
            {
                TypeDescriptor.AddAttributes(type, new TypeConverterAttribute(typeof(WsdlObjectConverter)));
            }
        }
    }
}
