using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServiceTestStudio.Views;

namespace WebServiceTestStudio
{
    public class MainWindowDesignViewModel
    {
        public MainWindowDesignViewModel()
        {
            //this.WsdlControlViewModel = new WsdlControlViewModel();
            ParametersViewViewModels = new List<ParametersViewViewModel>() 
            { 
                new ParametersViewViewModel(null, null, null, null) { MethodName = "one"},
                new ParametersViewViewModel(null, null, null, null) { MethodName = "two"}
            };
        }

        public WsdlControlViewModel WsdlControlViewModel { get; set; }
        public List<ParametersViewViewModel> ParametersViewViewModels { get; set; }
    }
}
