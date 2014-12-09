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
using System.Windows.Shapes;
using Microsoft.Practices.Prism.Mvvm;

namespace WebServiceTestStudio.Views
{
    /// <summary>
    /// Interaction logic for LookupWindow.xaml
    /// </summary>
    public partial class LookupWindow : Window, IView
    {
        public LookupWindow()
        {
            InitializeComponent();
        }

        public LookupWindow(LookupWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;                
        }
    }
}
