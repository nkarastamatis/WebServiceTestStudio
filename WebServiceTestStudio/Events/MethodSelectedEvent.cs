using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Practices.Prism.PubSubEvents;

namespace WebServiceTestStudio.Events
{
    /// <summary>
    /// Event used to create a new tab with method request parameters.
    /// </summary>
    public class MethodSelectedEvent : PubSubEvent<MethodInfo>
    {
    }

    public class MethodSelectedEventArgs : EventArgs
    {

    }
}
