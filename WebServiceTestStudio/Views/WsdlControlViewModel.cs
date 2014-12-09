using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceTestStudio.Controls;
using WebServiceTestStudio.Events;
using WebServiceTestStudio.Model;
using System.ComponentModel;

namespace WebServiceTestStudio.Views
{
    public class WsdlControlViewModel : BindableBase
    {
        private IEventAggregator eventAggregator;

        string selectedWsdlPath;
        List<string> fileHistory = new List<string>();
        List<object> filterList;
        object selectedFilter;
        List<MethodInfo> methods;
        MethodInfo selectedMethod;
        Dictionary<Type, List<MethodInfo>> methodsByType;
        BackgroundWorker loadWsdlBackgroundWorker = new BackgroundWorker();
        WsdlModel wsdlModel = new WsdlModel();

        public string SelectedWsdlPath
        {
            get { return selectedWsdlPath; }
            set { SetProperty<string>(ref selectedWsdlPath, value); }
        }

        public List<string> FileHistory
        {
            get { return fileHistory; }
            set { SetProperty<List<string>>(ref fileHistory, value); }
        }

        public List<object> FilterList
        {
            get { return filterList; }
            set { SetProperty<List<object>>(ref filterList, value); }
        }

        public object SelectedFilter
        {
            get { return selectedFilter; }
            set 
            { 
                SetProperty<object>(ref selectedFilter, value);

                if (selectedFilter is Type)
                {
                    List<MethodInfo> wsMethodList = null;
                    if (wsdlModel.MethodsByType.TryGetValue(selectedFilter as Type, out wsMethodList))
                    {
                        WsdlModel.ChangeProtocolType(selectedFilter as Type);
                        Methods = wsMethodList;
                        return;
                    }

                    Methods = wsdlModel.GetMethodsByType(selectedFilter as Type);
                }
                else
                {
                    Methods = wsdlModel.Methods;
                }
            }
        }

        public List<MethodInfo> Methods
        {
            get { return methods; }
            set { SetProperty<List<MethodInfo>>(ref methods, value); }
        }

        public MethodInfo SelectedMethod
        {
            get { return selectedMethod; }
            set { SetProperty<MethodInfo>(ref selectedMethod, value); }
        }

        public Dictionary<Type, List<MethodInfo>> MethodsByType
        {
            get { return methodsByType; }
            set { SetProperty<Dictionary<Type, List<MethodInfo>>>(ref methodsByType, value); }
        }

        public ICommand BrowseCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand MethodSelectedCommand { get; private set; }
        public ICommand FileSelectionChanged { get; private set; }
        private List<DelegateCommand> Commands { get; set; }
        
        public WsdlControlViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;

            FileHistory = new List<string>();
            BrowseCommand = new DelegateCommand(this.OnBrowse, this.CanExcecute);
            LoadCommand = new DelegateCommand(this.OnLoad, this.CanExcecute);
            MethodSelectedCommand = new DelegateCommand(this.OnMethodSelected, this.CanExcecute);
            FileSelectionChanged = new DelegateCommand(this.OnFileSelectionChanged, this.CanExcecute);

            Commands = new List<DelegateCommand>();
            Commands.Add(BrowseCommand as DelegateCommand);
            Commands.Add(LoadCommand as DelegateCommand);
            Commands.Add(MethodSelectedCommand as DelegateCommand);
            Commands.Add(FileSelectionChanged as DelegateCommand);

            LoadFileHistory();
            SelectedWsdlPath = FileHistory.FirstOrDefault();

            var evt = eventAggregator.GetEvent<InvokeEvent>();
            evt.Subscribe(wsdlModel.InvokeWebMethod);

            ParameterContextMenuGenerator.GetMethodsByType = wsdlModel.GetMethodsByType;

            loadWsdlBackgroundWorker.DoWork += delegate
            {
                wsdlModel.Initialize(selectedWsdlPath);
            };

            loadWsdlBackgroundWorker.RunWorkerCompleted += delegate
            {
                var filterList = new List<object>();
                filterList.AddRange(wsdlModel.MethodsByType.Keys);
                filterList.AddRange(wsdlModel.Classes);
                FilterList = filterList;

                SelectedFilter = FilterList.FirstOrDefault();
                MouseOverride.Remove(loadWsdlBackgroundWorker);
                RaiseCanExecuteChanged();
            };
        }

        private bool CanExcecute()
        {
            return !loadWsdlBackgroundWorker.IsBusy;
        }

        private void RaiseCanExecuteChanged()
        {
            foreach (var command in Commands)
                command.RaiseCanExecuteChanged();
        }

        ~WsdlControlViewModel()
        {
            var fileHistoryLocation = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WebServiceTestStudio",
                "fileHistory.xml");

            if (!Directory.Exists(Path.GetDirectoryName(fileHistoryLocation)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileHistoryLocation));
            }

            var serializer = new XmlSerializer(typeof(List<string>));
            using (var stream = File.OpenWrite(fileHistoryLocation))
            {
                serializer.Serialize(stream, FileHistory);
            }
        }

        private void LoadFileHistory()
        {
            var fileHistoryLocation = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WebServiceTestStudio",
                "fileHistory.xml");

            // Load Wsdl path history
            var fileHistSerializer = new XmlSerializer(typeof(List<string>));
            if (File.Exists(fileHistoryLocation))
            {
                try
                {
                    using (var stream = File.OpenRead(fileHistoryLocation))
                    {
                        FileHistory = (List<string>)fileHistSerializer.Deserialize(stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not load file history {0}", ex.Message);
                }
            }
        }


        public void OnBrowse()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "WSDL Files|*.wsdl";
            if (openFileDialog.ShowDialog() == true)
            {
                var newHistory = new List<string>(fileHistory);
                newHistory.Insert(0, openFileDialog.FileName);
                FileHistory = newHistory;
                SelectedWsdlPath = openFileDialog.FileName;
            }
        }

        public void OnLoad()
        {
            MouseOverride.Add(loadWsdlBackgroundWorker);
            loadWsdlBackgroundWorker.RunWorkerAsync();
            RaiseCanExecuteChanged();
        }

        public void OnMethodSelected()
        {
            var evt = eventAggregator.GetEvent<MethodSelectedEvent>();
            evt.Publish(SelectedMethod);
        }

        public void OnFileSelectionChanged()
        {
            var newList = new List<string>(fileHistory);
            newList.Remove(selectedWsdlPath);
            newList.Insert(0, selectedWsdlPath);
            FileHistory = newList;
        }
    }
}
