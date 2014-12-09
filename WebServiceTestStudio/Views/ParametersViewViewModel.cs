using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using System.Dynamic;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceTestStudio.Events;
using WebServiceTestStudio.Controls;
using System.ComponentModel;
using SettingsTracking;
using SettingsTracking.DataStoring;
using SettingsTracking.Serialization;

namespace WebServiceTestStudio.Views
{
    public class ParametersViewViewModel : BindableBase
    {
        string methodName;
        dynamic requestObject;
        dynamic resultObject;
        string requestXml;
        string responseXml;
        IContextMenuGenerator requestContextMenuGenerator;
        IContextMenuGenerator responseContextMenuGenerator;

        MethodInfo methodInfo;
        EventAggregator eventAggregator;
        BackgroundWorker invokeMethodBackgroundWorker = new BackgroundWorker();
        public string Guid { get; set; }

        public string LoadingResponse { get { return "Loading Response"; } }

        public IContextMenuGenerator RequestContextMenuGenerator
        {
            get { return requestContextMenuGenerator; }
            set { SetProperty<IContextMenuGenerator>(ref requestContextMenuGenerator, value); }
        }

        public IContextMenuGenerator ResponseContextMenuGenerator
        {
            get { return responseContextMenuGenerator; }
            set { SetProperty<IContextMenuGenerator>(ref responseContextMenuGenerator, value); }
        }

        public string MethodName
        {
            get { return methodName; }
            set { SetProperty<string>(ref methodName, value); }
        }

        public dynamic RequestObject
        {
            get { return requestObject; }
            //set { requestObject = value; }
            set { SetProperty<dynamic>(ref requestObject, value); }
        }

        public dynamic ResultObject
        {
            get { return resultObject; }
            //set { resultObject = value; }
            set { SetProperty<dynamic>(ref resultObject, value); }
        }

        public string RequestXml
        {
            get { return requestXml; }
            set { SetProperty<string>(ref requestXml, value); }
        }

        public string ResponseXml
        {
            get { return responseXml; }
            set { SetProperty<string>(ref responseXml, value); }
        }

        public void DisplayParameters(MethodInfo methodInfo)
        {
            // Input Parameters
            var parameters = methodInfo.GetParameters();
            dynamic expando = new ExpandoObject();
            var dictionary = expando as IDictionary<String, object>;
            foreach (var param in parameters)
            {
                dictionary[param.Name] = param.ParameterType.CreateObject();
            }

            MethodName = methodInfo.Name;
            //var contextMenu = ParamPropGridContextMenu.Add(requestControl.requestDataPropertyGrid);
            //contextMenu.SendParameter += OnSendParameter;
            RequestObject = expando;
        }

        public ParametersViewViewModel(
            MethodInfo methodInfo, 
            EventAggregator eventAggregator,
            IContextMenuGenerator requestContextMenuGenerator,
            IContextMenuGenerator responseContextMenuGenerator)
        {
            RequestContextMenuGenerator = requestContextMenuGenerator;
            ResponseContextMenuGenerator = responseContextMenuGenerator;
            this.eventAggregator = eventAggregator;
            this.methodInfo = methodInfo;
            if (methodInfo != null)
                DisplayParameters(this.methodInfo);

            Guid = System.Guid.NewGuid().ToString();

            invokeMethodBackgroundWorker.DoWork += (s, e) =>
                {
                    var parameters = requestObject as IDictionary<String, object>;

                    var evt = eventAggregator.GetEvent<InvokeEvent>();
                    var parameterArray = parameters.Values.ToArray();
                    var evtArgs = new InvokeEventArgs(methodInfo, ref parameterArray);
                    evt.Publish(evtArgs);

                    // Get the results
                    var result = evtArgs.Result;
                    dynamic expandoReturn = new ExpandoObject();
                    var dictionary = expandoReturn as IDictionary<String, object>;
                    dictionary["result"] = result ?? String.Empty;
                    ResultObject = expandoReturn;

                    RequestXml = evtArgs.RequestProperties.requestPayLoad;
                    ResponseXml = evtArgs.RequestProperties.responsePayLoad;
                };

            invokeMethodBackgroundWorker.RunWorkerCompleted += (s, e) =>
                {
                    MouseOverride.Remove(invokeMethodBackgroundWorker);                    
                };

            //SettingsTracker SettingsTracker = new SettingsTracker(
            //    new FileDataStore(Environment.SpecialFolder.ApplicationData, "ParametersViewViewModels.xml"),
            //    new ExpandoSerializer());

            //SettingsTracker.Configure(this)
            //    .AddProperties<ParametersViewViewModel>(vm => vm.RequestObject)
            //    .SetKey(methodInfo.Name)
            //    .Apply();
        }

        internal void Invoke()
        {
            if (invokeMethodBackgroundWorker.IsBusy)
                return;

            // Clear previous Response
            ResultObject = null;
            RequestXml = "Processing Request";
            ResponseXml = String.Empty;

            MouseOverride.Add(invokeMethodBackgroundWorker);
            invokeMethodBackgroundWorker.RunWorkerAsync();
        }
    }


}
