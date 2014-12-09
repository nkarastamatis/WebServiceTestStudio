using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;

namespace WebServiceTestStudio.Events
{
    public class CurrentProtocolChangedEvent : PubSubEvent<CurrentProtocolChangedEventArgs>
    {
    }

    public class CurrentProtocolChangedEventArgs : EventArgs
    {

    }
}
