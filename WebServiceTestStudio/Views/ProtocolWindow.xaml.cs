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
    /// Interaction logic for ProtocolWindow.xaml
    /// </summary>
    public partial class ProtocolWindow : Window, IView
    {
        public ProtocolWindow()
        {
            InitializeComponent();
        }

        public ProtocolWindow(ProtocolWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void LoadSettings()
        {
            Services.SettingsTracker.Configure(this)
                .AddProperties<ProtocolWindow>(w => w.Height, w => w.Width, w => w.Left, w => w.Top, w => w.WindowState, w => w.LabelRatio)
                .SetKey("ProtocolWindow")//not really needed since only one instance of MainWindow will ever exist
                .Apply();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Model.WsdlModel.Save();
        }

        public double LabelRatio
        {
            get 
            {
                var propertyGrid = wpfPropertyGrid.PropertyGrid;
                var type = propertyGrid.Controls[2].GetType();
                var field = type.GetField("labelRatio");
                var labelRatio = (double)field.GetValue(propertyGrid.Controls[2]);
                return labelRatio;
            }

            set
            {
                var propertyGrid = wpfPropertyGrid.PropertyGrid;
                var type = propertyGrid.Controls[2].GetType();
                var field = type.GetField("labelRatio");
                field.SetValue(propertyGrid.Controls[2], value);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }
    }
}
