using System.Windows.Controls;
using Microsoft.Practices.Prism.Mvvm;

namespace WebServiceTestStudio.Views
{
    /// <summary>
    /// Interaction logic for ParametersView.xaml
    /// </summary>
    public partial class ParametersView : UserControl, IView
    {
        public ParametersView()
        {
            InitializeComponent();
        }

        public ParametersView(ParametersViewViewModel parametersViewViewModel)
        {
            InitializeComponent();
            DataContext = parametersViewViewModel;
        }
    }
}
