using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceStudio;

namespace WebServiceTestStudio.Events
{
    /// <summary>
    /// Event used to notify the business logic which method
    /// is being invoked with it's parameters.
    /// </summary>
    public class InvokeEvent : PubSubEvent<InvokeEventArgs>
    {
    }

    public class InvokeEventArgs : EventArgs
    {
        public MethodInfo MethodInfo { get; private set; }
        public object[] Parameters { get; private set; }
        public RequestProperties RequestProperties {get; set;}
        public object Result { get; set; }

        public InvokeEventArgs(MethodInfo methodInfo, ref object[] parameters)
        {
            MethodInfo = methodInfo;
            Parameters = parameters;
        }
    }
}
