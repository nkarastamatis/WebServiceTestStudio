using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web.Services.Protocols;
using WebServiceTestStudio.Model;

namespace WebServiceTestStudio
{
    public static class Extentions
    {
        /// <summary>
        /// Helper function to use the appropriate CreateInstance
        /// method based on type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateObject(this Type type)
        {
            object obj = null;
            if (type == typeof(String))
                obj = Activator.CreateInstance(type, '\0', 0);
            else if (type.BaseType == typeof(Array))
                obj = Activator.CreateInstance(type, 1);
            else if (type.Namespace != "System")
                obj = WsdlModel.ProxyAssembly.CreateInstance(type.Name);

            return obj;
        }

        /// <summary>
        /// Performs a deep copy of the source object to the copy object.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="copy"></param>
        public static void Copy(this object source, ref object copy)
        {
            if (source == null)
                return;

            Type type = source.GetType();
            if (!type.IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            foreach (var fieldInfo in type.GetFields())
            {
                var sourceArray = fieldInfo.GetValue(source) as Array;
                if (sourceArray != null)
                {
                    // Create and set an arry of the same type and size
                    var elemType = fieldInfo.FieldType.GetElementType();
                    var copyArray = Array.CreateInstance(elemType, sourceArray.Length);
                    fieldInfo.SetValue(copy, copyArray);

                    // For each element, copy and set
                    for (int ix = 0; ix < sourceArray.Length; ix++)
                    {
                        var copyItem = elemType.CreateObject();
                        sourceArray.GetValue(ix).Copy(ref copyItem);
                        copyArray.SetValue(copyItem, ix);
                    }

                    continue;
                }

                if (fieldInfo.FieldType.Namespace == "System" || fieldInfo.FieldType.BaseType.Name == "Enum")
                    fieldInfo.SetValue(copy, fieldInfo.GetValue(source));
                else
                {
                    object childClone = fieldInfo.GetValue(copy);
                    if (childClone == null)
                        childClone = fieldInfo.FieldType.CreateObject();
                    fieldInfo.GetValue(source).Copy(ref childClone);
                    fieldInfo.SetValue(copy, childClone);
                }
            }
        }

        public static bool SettingsEqual(this HttpWebClientProtocol lhs, HttpWebClientProtocol rhs)
        {
            if (
                lhs.AllowAutoRedirect == rhs.AllowAutoRedirect &&
                lhs.EnableDecompression == rhs.EnableDecompression &&
                lhs.ConnectionGroupName == rhs.ConnectionGroupName &&
                lhs.PreAuthenticate == rhs.PreAuthenticate &&
                lhs.Timeout == rhs.Timeout &&
                lhs.Url == rhs.Url &&
                lhs.UseDefaultCredentials == rhs.UseDefaultCredentials)
                return true;

            return false;
        }
    }
}

namespace System.Collections.ObjectModel
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";
        private const string KeysName = "Keys";
        private const string ValuesName = "Values";

        private IDictionary<TKey, TValue> _Dictionary;
        protected IDictionary<TKey, TValue> Dictionary
        {
            get { return _Dictionary; }
        }

        #region Constructors
        public ObservableDictionary()
        {
            _Dictionary = new Dictionary<TKey, TValue>();
        }
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _Dictionary = new Dictionary<TKey, TValue>(dictionary);
        }
        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            _Dictionary = new Dictionary<TKey, TValue>(comparer);
        }
        public ObservableDictionary(int capacity)
        {
            _Dictionary = new Dictionary<TKey, TValue>(capacity);
        }
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }
        public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }
        #endregion

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return Dictionary.Keys; }
        }

        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            TValue value;
            Dictionary.TryGetValue(key, out value);
            var removed = Dictionary.Remove(key);
            if (removed)
                //OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
                OnCollectionChanged();

            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }


        public ICollection<TValue> Values
        {
            get { return Dictionary.Values; }
        }


        public TValue this[TKey key]
        {
            get
            {
                return Dictionary[key];
            }
            set
            {
                Insert(key, value, false);
            }
        }


        #endregion


        #region ICollection<KeyValuePair<TKey,TValue>> Members


        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Insert(item.Key, item.Value, true);
        }


        public void Clear()
        {
            if (Dictionary.Count > 0)
            {
                Dictionary.Clear();
                OnCollectionChanged();
            }
        }


        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Dictionary.Contains(item);
        }


        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Dictionary.CopyTo(array, arrayIndex);
        }


        public int Count
        {
            get { return Dictionary.Count; }
        }


        public bool IsReadOnly
        {
            get { return Dictionary.IsReadOnly; }
        }


        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }


        #endregion


        #region IEnumerable<KeyValuePair<TKey,TValue>> Members


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }


        #endregion


        #region IEnumerable Members


        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Dictionary).GetEnumerator();
        }


        #endregion


        #region INotifyCollectionChanged Members


        public event NotifyCollectionChangedEventHandler CollectionChanged;


        #endregion


        #region INotifyPropertyChanged Members


        public event PropertyChangedEventHandler PropertyChanged;


        #endregion


        public void AddRange(IDictionary<TKey, TValue> items)
        {
            if (items == null) throw new ArgumentNullException("items");


            if (items.Count > 0)
            {
                if (Dictionary.Count > 0)
                {
                    if (items.Keys.Any((k) => Dictionary.ContainsKey(k)))
                        throw new ArgumentException("An item with the same key has already been added.");
                    else
                        foreach (var item in items) Dictionary.Add(item);
                }
                else
                    _Dictionary = new Dictionary<TKey, TValue>(items);


                OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
            }
        }


        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null) throw new ArgumentNullException("key");


            TValue item;
            if (Dictionary.TryGetValue(key, out item))
            {
                if (add) throw new ArgumentException("An item with the same key has already been added.");
                if (Equals(item, value)) return;
                Dictionary[key] = value;


                OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, item));
            }
            else
            {
                Dictionary[key] = value;

                OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
            }
        }


        private void OnPropertyChanged()
        {
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnPropertyChanged(KeysName);
            OnPropertyChanged(ValuesName);
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        private void OnCollectionChanged()
        {
            OnPropertyChanged();
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
        {
            OnPropertyChanged();
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, changedItem));
        }


        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
        {
            OnPropertyChanged();
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
        }


        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
        {
            OnPropertyChanged();
            if (CollectionChanged != null) CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItems));
        }
    }
}  
