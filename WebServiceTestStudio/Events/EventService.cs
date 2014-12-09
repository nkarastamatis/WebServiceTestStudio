using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;

namespace WebServiceTestStudio.Events
{
    /// <summary>
    /// 
    /// </summary>
    public static class EventService
    {
        // EventAggregator singleton
        static EventAggregator _instance;

        /// <summary>
        /// Gets the EventAggregator singleton
        /// </summary>
        public static EventAggregator EventAggregator
        {
            get
            {
                if (_instance == null)
                    _instance = new EventAggregator();

                return _instance;
            }
        }

        // Application's Clipboard
        // This object must be copied to the paste object
        // because it's just a reference.
        public static object ClipboardObject { get; set; }
    }
}
