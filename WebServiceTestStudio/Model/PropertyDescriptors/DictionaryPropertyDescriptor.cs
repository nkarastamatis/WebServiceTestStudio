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
    /// PropertyDescriptor for Dictionary entries.
    /// </summary>
    class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        IDictionary<String, object> _dictionary;
        String _key;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d">The dictionary containing the entry.</param>
        /// <param name="key">The key of the key-value pair use wants to describe.</param>
        internal DictionaryPropertyDescriptor(IDictionary<String, object> d, String key)
            : base(key.ToString(), null)
        {
            
            _dictionary = d;
            _key = key;
        }
        
        public override Type PropertyType
        {
            get 
            {
                if (_dictionary[_key] != null)
                    return _dictionary[_key].GetType();
                else
                    return typeof(object);
            }
        }
        public override void SetValue(object component, object value)
        {
            _dictionary[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key];
        }
        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
