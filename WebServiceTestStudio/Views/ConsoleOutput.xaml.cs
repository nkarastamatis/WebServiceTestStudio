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
using System.IO;

namespace WebServiceTestStudio.Views
{
    /// <summary>
    /// Interaction logic for ConsoleOutput.xaml
    /// </summary>
    public partial class ConsoleOutput : Window
    {
        public ConsoleOutput()
        {
            InitializeComponent();

            //set up tracking and apply state for the main window
            Services.SettingsTracker.Configure(this)
                .AddProperties<ConsoleOutput>(w => w.Height, w => w.Width, w => w.Left, w => w.Top, w => w.WindowState)
                .SetKey("ConsoleOutput")//not really needed since only one instance of MainWindow will ever exist
                .Apply();

            Console.SetOut(new TextBoxOutputter(ConsoleTextBox));
        }

        public class TextBoxOutputter : TextWriter
        {
            TextBox textBox = null;

            public TextBoxOutputter(TextBox output)
            {
                textBox = output;
            }

            public override void Write(char value)
            {
                base.Write(value);
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.AppendText(value.ToString());
                    textBox.ScrollToEnd();
                }));
            }

            public override Encoding Encoding
            {
                get { return System.Text.Encoding.UTF8; }
            }
        }
    }
}
