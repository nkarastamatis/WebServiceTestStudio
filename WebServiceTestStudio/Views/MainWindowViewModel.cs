using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WebServiceTestStudio.Views
{
    public class MainWindowViewModel : BindableBase
    {
        WsdlControlViewModel wsdlControlViewModel;
        ObservableCollection<ParametersViewViewModel> parametersViewViewModels;
        ParametersViewViewModel selectedParametersViewViewModel;

        EventAggregator eventAggregator;

        public  WsdlControlViewModel WsdlControlViewModel
        {
            get { return wsdlControlViewModel; }
            set { SetProperty<WsdlControlViewModel>(ref wsdlControlViewModel, value); }
        }

        public ObservableCollection<ParametersViewViewModel> ParametersViewViewModels
        {
            get { return parametersViewViewModels; }
            set { SetProperty<ObservableCollection<ParametersViewViewModel>>(ref parametersViewViewModels, value); }
        }

        public ParametersViewViewModel SelectedParametersViewViewModel
        {
            get { return selectedParametersViewViewModel; }
            set { SetProperty<ParametersViewViewModel>(ref selectedParametersViewViewModel, value); Controls.UIHelper.SetBusyState(); }
        }

        public ObservableCollection<System.Windows.Window> OpenWindows { get; set; }

        public ICommand CloseTabCommand { get; private set; }
        public DelegateCommand InvokeCommand { get; private set; }
        public DelegateCommand InvokeAllCommand { get; private set; }

        public MainWindowViewModel()
        {
            ParametersViewViewModels = new ObservableCollection<ParametersViewViewModel>();
            OpenWindows = new ObservableCollection<System.Windows.Window>();

            eventAggregator = EventService.EventAggregator;
            CloseTabCommand = new DelegateCommand<string>(this.OnCloseTab);
            WsdlControlViewModel = new WsdlControlViewModel(eventAggregator);
            InvokeCommand = new DelegateCommand(this.OnInvoke, this.OnInvoke_CanExecute);
            InvokeAllCommand = new DelegateCommand(this.OnInvokeAll, this.OnInvoke_CanExecute);

            var methodSelectedEvent = eventAggregator.GetEvent<MethodSelectedEvent>();
            methodSelectedEvent.Subscribe(MethodSelected);

            var sendParameterToNewMethodEvent = eventAggregator.GetEvent<SendParameterToNewMethodEvent>();
            sendParameterToNewMethodEvent.Subscribe(SendParameterToNewMethod);
        }

        private void OnCloseTab(string guid)
        {
            var item = ParametersViewViewModels.FirstOrDefault(vm => vm.Guid == guid);
            if (item != null)
            {
                ParametersViewViewModels.Remove(item);
                SelectedParametersViewViewModel = ParametersViewViewModels.FirstOrDefault();
            }

            RaiseCanExecuteChanged();
        }

        private void AddParametersViewViewModel(MethodInfo methodInfo)
        {
            ParametersViewViewModels.Add(new ParametersViewViewModel(
                methodInfo, 
                eventAggregator, 
                new Controls.ParameterContextMenuGenerator(),
                new Controls.ParameterContextMenuGenerator()));
            SelectedParametersViewViewModel = ParametersViewViewModels.LastOrDefault();

            RaiseCanExecuteChanged();
        }

        private void RaiseCanExecuteChanged()
        {
            InvokeCommand.RaiseCanExecuteChanged();
            InvokeAllCommand.RaiseCanExecuteChanged();
        }

        private void MethodSelected(MethodInfo obj)
        {
            AddParametersViewViewModel(obj);
        }

        private void SendParameterToNewMethod(SendParameterToNewMethodEventArgs args)
        {
            AddParametersViewViewModel(args.SendToMethodInfo);
            var newEmptyObject = selectedParametersViewViewModel.RequestObject as IDictionary<String, object>;
            foreach (var obj in newEmptyObject)
            {
                if (obj.Value.GetType() == args.CopyObject.GetType())
                {
                    //existingObj[obj.Key] = copyObject.Clone();
                    var value = newEmptyObject[obj.Key];
                    args.CopyObject.Copy(ref value);
                    break;
                }
            } 
        }

        private bool OnInvoke_CanExecute()
        {
            return ParametersViewViewModels != null && ParametersViewViewModels.Any();
        }

        public void OnInvoke()
        {
            SelectedParametersViewViewModel.Invoke();
        }

        public void OnInvokeAll()
        {
            foreach (var vm in ParametersViewViewModels)
                vm.Invoke();
        }

        public void OpenProtocolSettings()
        {
            if (ActivateExistingWindow(typeof(ProtocolWindow)))
                return;

            var protocolWindow = new ProtocolWindow(new ProtocolWindowViewModel());
            protocolWindow.Tag = Commands.MainWindowCommands.OpenProtocolSettings;
            OpenWindows.Add(protocolWindow);
            protocolWindow.Show();
        }

        public void OpenHelp()
        {
            if (ActivateExistingWindow(typeof(HelpWindow)))
                return;

            var helpWindow = new HelpWindow();
            helpWindow.Tag = Commands.MainWindowCommands.OpenHelp;
            OpenWindows.Add(helpWindow);
            helpWindow.Show();
        }

        public void OpenConsoleOutput()
        {
            if (ActivateExistingWindow(typeof(ConsoleOutput)))
                return;

            var consoleOutput = new ConsoleOutput();
            consoleOutput.Tag = Commands.MainWindowCommands.OpenConsoleOutput;
            OpenWindows.Add(consoleOutput);
            consoleOutput.Show();
        }

        private bool ActivateExistingWindow(Type type)
        {
            var existingWindow = OpenWindows.FirstOrDefault(w => w.GetType() == type);

            if (existingWindow == null)
                return false;

            return existingWindow.Activate();
        }
    }
}
