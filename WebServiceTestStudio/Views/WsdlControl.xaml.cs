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

namespace WebServiceTestStudio.Views
{
    /// <summary>
    /// Interaction logic for WsdlView.xaml
    /// </summary>
    public partial class WsdlControl : UserControl, IView
    {
        public WsdlControl()
        {
            InitializeComponent();
        }

        public WsdlControl(WsdlControlViewModel wsdlControlViewModel)
        {
            InitializeComponent();
            DataContext = wsdlControlViewModel;
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as WsdlControlViewModel;

            if (vm != null)
                vm.MethodSelectedCommand.Execute(this.Content);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as WsdlControlViewModel;

            if (vm != null)
                vm.FileSelectionChanged.Execute(this.Content);
        }

        private void ListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = DataContext as WsdlControlViewModel;

                if (vm != null)
                    vm.MethodSelectedCommand.Execute(this.Content);
            }
        }
    }
}
