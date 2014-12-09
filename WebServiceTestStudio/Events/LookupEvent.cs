using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceStudio;
using WebServiceTestStudio.Views;

namespace WebServiceTestStudio.Events
{
    /// <summary>
    /// Show a lookup window so the user can populate request objects.
    /// </summary>
    public class LookupEvent : PubSubEvent<LookupEventArgs>
    {
        public override void Publish(LookupEventArgs payload)
        {
            base.Publish(payload);
            var lookUpSource = payload.Object;
            var vm = new LookupWindowViewModel(ref lookUpSource, payload.LookupMethodInfo, payload.RefreshPropertyGrid);
            var lookupWindow = new WebServiceTestStudio.Views.LookupWindow(vm);
            lookupWindow.Title = String.Format("Lookup {0} via {1}",
                payload.Object.GetType().ToString(),
                payload.LookupMethodInfo.Name);
            lookupWindow.Show();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LookupEventArgs : EventArgs
    {
        public MethodInfo LookupMethodInfo { get; private set; }
        public object Object { get; set; }
        public Action RefreshPropertyGrid { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Reference to the object the user wants to populate.</param>
        /// <param name="lookupMethodInfo">The method chosen to lookup the desired object.</param>
        /// <param name="refreshPropertyGrid">
        ///     The refresh action need to repaint the referenced object after copy and paste.
        /// </param>
        public LookupEventArgs(ref object obj, MethodInfo lookupMethodInfo, Action refreshPropertyGrid)
        {
            LookupMethodInfo = lookupMethodInfo;
            Object = obj;
            RefreshPropertyGrid = refreshPropertyGrid;
        }
    }
}
