using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using WebServiceStudio;
using WebServiceTestStudio.Model;

namespace WebServiceTestStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        IUnityContainer _container = new UnityContainer();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // The Prism library is used to support the MVVM design pattern used in this application.
            // As you may have noticed, the default folder structure is not used here. Instead, the 
            // views and viewmodels are in the same folder. It made more sense to me having the two related
            // files right next to each other in the solution explorer.
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewName = viewType.FullName;
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = String.Format(CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName);
                return Type.GetType(viewModelName); 
            });

            ViewModelLocationProvider.SetDefaultViewModelFactory((type) =>
            {
                return _container.Resolve(type);
            });

            WSSWebRequestCreate.RegisterPrefixes();
            TypeDescriptorModifier.modifyType(typeof(System.Dynamic.ExpandoObject));

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Test();
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var filePath = args.Name.Split(',')[0];
            filePath = System.IO.Path.Combine("C:\\Users\\nkarastamatis\\AppData\\Local\\Temp", filePath + ".dll");
            if (System.IO.File.Exists(filePath))
                return Assembly.LoadFile(filePath);
            return null;
        }

        private void Test()
        {
            int COUNT = 1000000;

            var list = new System.Collections.Generic.List<int>();
            var hashset = new System.Collections.Generic.HashSet<int>();
            var array = new int[COUNT];

            for (int i = 0; i < COUNT; i++)
            {
                list.Add(i);
                hashset.Add(i);
                array[i] = i;
            }

            Enumerate(list);
            Enumerate(hashset);

            

            var stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            //foreach (var e in list)
            //{
            //    list.Contains(e);
            //}
            //stopwatch.Stop();
            //Console.WriteLine(String.Format("List took {0}", stopwatch.Elapsed));

            //stopwatch.Reset();
            //stopwatch.Start();
            //foreach (var e in hashset)
            //{
            //    hashset.Contains(e);                
            //}
            //stopwatch.Stop();
            //Console.WriteLine(String.Format("Hashset Took {0}", stopwatch.Elapsed));

            int total = 0;
            stopwatch.Reset();
            stopwatch.Start();
            foreach (var e in array)
            {
                total += e;
            }
            stopwatch.Stop();
            Console.WriteLine(String.Format("Foreach Took {0}", stopwatch.Elapsed));

            total = 0;
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i<array.Length; i++)
            {
                total += array[i];
            }
            stopwatch.Stop();
            Console.WriteLine(String.Format("For Took {0}", stopwatch.Elapsed));

            return;
        }

        

        private void Enumerate(System.Collections.Generic.IEnumerable<int> enumerable)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            foreach (var e in enumerable)
            {
                int integer = e;
            }

            stopwatch.Stop();
            Console.WriteLine(String.Format("{0} took {1}", enumerable.GetType().Name, stopwatch.Elapsed));
        }

    }
}
