using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

namespace WebServiceTestStudio.Model
{
    public class WsdlFieldDescriptor : PropertyDescriptor
    {
        private FieldInfo _fi;
        public WsdlFieldDescriptor(FieldInfo fi, Attribute[] attributes)
            : base(fi.Name, null)
        {
            _fi = fi;
        }

        public override bool SupportsChangeEvents
        {
            get
            {
                return true;
            }
        }

        public override bool IsReadOnly
        {

            get { return false; }
        }

        public override void ResetValue(object component) { }

        public override bool CanResetValue(object component) { return false; }
        public override bool ShouldSerializeValue(object component)
        { return true; }
        public override Type ComponentType
        {
            get
            {
                return typeof(Object);
            }
        }
        public override Type PropertyType
        {
            get
            {
                return _fi.FieldType;
            }
        }

        public override object GetValue(object component)
        {
            if (component.GetType().GetField(_fi.Name) == null)
                return null;

            var value = _fi.GetValue(component);
            var type = _fi.FieldType;
            if (value == null && type.GetConstructor(Type.EmptyTypes) != null)
            {
                value = Activator.CreateInstance(type);
                _fi.SetValue(component, value);
                OnValueChanged(component, EventArgs.Empty);
            }
            return value;
        }
        public override void SetValue(object component, object value)
        {
            _fi.SetValue(component, value);
            OnValueChanged(component, EventArgs.Empty);
        }
    }
}
