using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.UnityExtensions;
using WebServiceTestStudio.Events;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using WebServiceTestStudio.Controls;
using System.Web.Services.Protocols;
using WebServiceTestStudio.Model;

namespace WebServiceTestStudio.Views
{
    public class ProtocolWindowViewModel : BindableBase
    {
        HttpWebClientProtocol selectedProtocol;
        ObservableDictionary<string, HttpWebClientProtocol> protocolsByName;

        public HttpWebClientProtocol SelectedProtocol 
        {
            get { return selectedProtocol; }
            set 
            {
                if (value == null) return;

                SetProperty<HttpWebClientProtocol>(ref selectedProtocol, value);
                TypeDescriptorModifier.modifyType(selectedProtocol.GetType());
                WsdlModel.CurrentProtocol = selectedProtocol;

                if (WsdlModel.LastProtocolNameByType.ContainsKey(ProtocolType))
                    WsdlModel.LastProtocolNameByType[ProtocolType] = SelectedProtocolItem.Key;
                else
                    WsdlModel.LastProtocolNameByType.Add(ProtocolType, SelectedProtocolItem.Key);
            }
        }

        public KeyValuePair<string, HttpWebClientProtocol> SelectedProtocolItem { get; set; }

        public ObservableDictionary<string, HttpWebClientProtocol> ProtocolsByName
        {
            get { return protocolsByName; }
            set { SetProperty<ObservableDictionary<string, HttpWebClientProtocol>>(ref protocolsByName, value); }
        }

        public ICommand AddProtocolCommand { get; private set; }

        public Type ProtocolType { get; private set; }

        public ProtocolWindowViewModel()
        {
            ProtocolsByName = new ObservableDictionary<string, HttpWebClientProtocol>();
            AddProtocolCommand = new DelegateCommand(OnAddProtocol);

            InitializeProperties();

            var evt = EventService.EventAggregator.GetEvent<CurrentProtocolChangedEvent>();
            evt.Subscribe(OnCurrentProtocolChanged);
        }

        private void InitializeProperties()
        {
            ProtocolType = WsdlModel.CurrentProtocol.GetType();
            ProtocolsByName = WsdlModel.CurrentProtocolsByName;

            string lastProtocolName;
            WsdlModel.LastProtocolNameByType.TryGetValue(ProtocolType, out lastProtocolName);

            HttpWebClientProtocol lastSelectedProtocol;
            if (!String.IsNullOrEmpty(lastProtocolName) &&
                ProtocolsByName.TryGetValue(lastProtocolName, out lastSelectedProtocol))
                SelectedProtocol = lastSelectedProtocol;
            else
                SelectedProtocol = ProtocolsByName.FirstOrDefault().Value;
            
        }

        private void OnCurrentProtocolChanged(CurrentProtocolChangedEventArgs obj)
        {
            InitializeProperties();
        }

        private void OnAddProtocol()
        {
            var newNameDlg = new NewProtocolName();
            var ret = newNameDlg.ShowDialog();
            if ((bool)ret)
            {
                var key = newNameDlg.ProtocolName.Text;
                if (!ProtocolsByName.ContainsKey(key))
                {
                    var newProtocol = ProtocolType.CreateObject() as HttpWebClientProtocol;
                    ProtocolsByName.Add(key, newProtocol);
                    SelectedProtocol = newProtocol;
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        String.Format(
                        "List already contains {0}",
                        key));
                }
            }
        }

    }
}
