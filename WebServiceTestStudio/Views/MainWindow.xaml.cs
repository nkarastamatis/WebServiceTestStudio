using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism;

namespace WebServiceTestStudio.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        public MainWindow()
        {
            InitializeComponent();

            Services.SettingsTracker.Configure(this)
                .AddProperties<MainWindow>(w => w.Height, w => w.Width, w => w.Left, w => w.Top, w => w.WindowState, w => w.Column1Width)
                .SetKey("MainWindow")//not really needed since only one instance of MainWindow will ever exist
                .Apply();
        }

        public double Column1Width
        {
            get
            {
                return Column1.Width.Value;
            }

            set
            {
                Column1.Width = new GridLength(value);
            }
        }

        private void Tab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                var vm = DataContext as MainWindowViewModel;
                var guid = (sender as StackPanel).Tag;
                if (vm != null)
                    vm.CloseTabCommand.Execute(guid);
            }
        }

        private void OpenProtocolSettings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = WebServiceTestStudio.Model.WsdlModel.CurrentProtocol != null;
        }

        private void OpenProtocolSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null)
                vm.OpenProtocolSettings();
        }

        private void OpenHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null)
                vm.OpenHelp();
        }

        private void OpenConsoleOutput_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenConsoleOutput_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null)
                vm.OpenConsoleOutput();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

    }
}
