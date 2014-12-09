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
    /// Event used to send a copy of an object to a new request.
    /// </summary>
    public class SendParameterToNewMethodEvent : PubSubEvent<SendParameterToNewMethodEventArgs>
    {
    }

    public class SendParameterToNewMethodEventArgs : EventArgs
    {
        public MethodInfo SendToMethodInfo { get; private set; }
        public object CopyObject { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="copyObject">The object you want to copy.</param>
        /// <param name="sendToMethodInfo">The new request you want to make with copyObject.</param>
        public SendParameterToNewMethodEventArgs(object copyObject, MethodInfo sendToMethodInfo)
        {
            SendToMethodInfo = sendToMethodInfo;
            CopyObject = copyObject;
        }
    }
}
