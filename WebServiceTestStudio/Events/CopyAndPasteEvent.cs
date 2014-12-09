using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceStudio;
using WebServiceTestStudio.Views;

namespace WebServiceTestStudio.Events
{
    /// <summary>
    /// Event to used to tell the Lookup window it time to copy the clipboard
    /// object to the lookup object.
    /// </summary>
    public class CopyAndPasteEvent : PubSubEvent<CopyAndPasteEventArgs>
    {

    }

    public class CopyAndPasteEventArgs : EventArgs
    {

        public CopyAndPasteEventArgs()
        {

        }
    }
}
