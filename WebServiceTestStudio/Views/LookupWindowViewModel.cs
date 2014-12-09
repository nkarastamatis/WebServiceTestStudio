using System;
using System.Collections.Generic;
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

namespace WebServiceTestStudio.Views
{
    public class LookupWindowViewModel : BindableBase
    {
        public ParametersViewViewModel ParametersViewViewModel { get; set; }
        public object LookupSource { get; set; }
        Action RefreshPropertyGrid;

        public ICommand InvokeCommand { get; private set; }

        public LookupWindowViewModel(ref object lookupSource, MethodInfo lookupMethod, Action refreshPropertyGrid)
        {
            LookupSource = lookupSource;
            ParametersViewViewModel = 
                new ParametersViewViewModel(
                    lookupMethod, 
                    EventService.EventAggregator, 
                    null,
                    new LookupContextMenuGenerator());

            InvokeCommand = new DelegateCommand(this.OnInvoke);
            RefreshPropertyGrid = refreshPropertyGrid;

            var evt = EventService.EventAggregator.GetEvent<CopyAndPasteEvent>();
            evt.Subscribe(OnCopyAndPaste);
        }

        public void OnInvoke()
        {
            ParametersViewViewModel.Invoke();
        }

        private void OnCopyAndPaste(CopyAndPasteEventArgs obj)
        {
            var copyToObj = LookupSource;
            var copyFromObj = EventService.ClipboardObject;
            if (copyFromObj == null)
                return;

            if (copyFromObj.GetType() == copyToObj.GetType())
            {
                copyFromObj.Copy(ref copyToObj);
                if (RefreshPropertyGrid != null)
                    RefreshPropertyGrid();
            }
            else
            {
                System.Windows.MessageBox.Show(
                        String.Format(
                        "Can not copy {0} to {1}",
                        copyFromObj.GetType(),
                        copyToObj.GetType()));
            }
        }
    }
}
