using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebServiceTestStudio.Commands
{
    public class MainWindowCommands
    {
        public static RoutedUICommand OpenProtocolSettings =
            new RoutedUICommand(
                "Open Protocol Settings",
                "OpenProtocolSettings",
                typeof(MainWindowCommands));

        public static RoutedUICommand OpenConsoleOutput =
            new RoutedUICommand(
                "Open Console Output",
                "OpenConsoleOutput",
                typeof(MainWindowCommands));

        public static RoutedUICommand OpenHelp =
            new RoutedUICommand(
                "Open Help",
                "OpenHelp",
                typeof(MainWindowCommands));
    }
}
