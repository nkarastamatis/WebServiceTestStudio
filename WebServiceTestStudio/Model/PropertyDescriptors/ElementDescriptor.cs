using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

namespace WebServiceTestStudio.Model
{
    /// <summary>
    /// PropertyDescriptor for Array Elements.
    /// </summary>
    public class ElementDescriptor : PropertyDescriptor
    {
        private Type _elementType;
        private Array _collection;
        private int _index = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementType">Type of containing elements.</param>
        /// <param name="collection">Collection containing the element.</param>
        /// <param name="index">Index of element to describe.</param>
        public ElementDescriptor(Type elementType, Array collection, int index)
            : base(elementType.Name, null)
        {
            _elementType = elementType;
            _collection = collection;
            _index = index;
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
                return typeof(Array);
            }
        }
        public override Type PropertyType
        {
            get
            {
                return _elementType;
            }
        }

        public override string DisplayName
        {
            get
            {
                return base.DisplayName + " #" + _index;
            }
        }

        public override object GetValue(object component)
        {
            var value = _collection.GetValue(_index);
            if (value == null)
            {
                value = Activator.CreateInstance(PropertyType);
                _collection.SetValue(value, _index);
            }

            return value;
        }
        public override void SetValue(object component, object value)
        {
            _collection.SetValue(value, _index);
            OnValueChanged(component, EventArgs.Empty);
        }
    }
}
