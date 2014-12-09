using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Services.Protocols;
using WebServiceStudio;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.IO;

namespace WebServiceTestStudio.Model
{
    /// <summary>
    /// Main Application Model contains the WSDL info, logic
    /// to read a WSDL file and invoke a method.
    /// </summary>
    public class WsdlModel
    {
        private string wsdlPath;
        public List<Type> Classes;
        public List<MethodInfo> Methods;
        public Dictionary<Type, List<MethodInfo>> MethodsByType;
        public static ObservableDictionary<string, HttpWebClientProtocol> CurrentProtocolsByName { get; set; }
        public static Dictionary<Type, ObservableDictionary<string, HttpWebClientProtocol>> ProtocolSettingsByType { get; set; }
        public static object SessionObject { get; set; }
        public static Dictionary<Type, string> LastProtocolNameByType { get; set; }

        public Wsdl Wsdl;
        private static HttpWebClientProtocol currentProtocol;
        // Made static for easy access in the application. If we ever
        // want to handle a wsdl with multiple services then we will need
        // to make sure this is properly assigned before invoke or find 
        // another way get get the applicable protocol before invoke.
        public static HttpWebClientProtocol CurrentProtocol
        {
            get { return currentProtocol; }
            set 
            {
                currentProtocol = value;
            }
        }

        public static void ChangeProtocolType(Type type)
        {
            string name;
            LastProtocolNameByType.TryGetValue(type, out name);
            name = name ?? DefaultProtocolName;

            CurrentProtocolsByName = ProtocolSettingsByType[type];
            CurrentProtocol = CurrentProtocolsByName[name];

            var evt = Events.EventService.EventAggregator.GetEvent<Events.CurrentProtocolChangedEvent>();
            evt.Publish(null);
        }

        public static Assembly ProxyAssembly;

        public event EventHandler WsdlModelInitialized;
        
        public WsdlModel()
        {
            Classes = new List<Type>();
            Methods = new List<MethodInfo>();
            MethodsByType = new Dictionary<Type, List<MethodInfo>>();
            ProtocolSettingsByType = new Dictionary<Type, ObservableDictionary<string, HttpWebClientProtocol>>();
            LastProtocolNameByType = new Dictionary<Type, string>();
            WsdlPath = "";
        }

        public void Initialize(string path)
        {
            Console.WriteLine("Generating wsdl.");
            var wsdl = new Wsdl();
            wsdl.Paths.Add(path);
            wsdl.Generate();
            Wsdl = wsdl;
            WsdlModel.ProxyAssembly = Wsdl.ProxyAssembly;

            Console.WriteLine("Generating classes and methods.");
            GenerateClassesAndMethods();

            WsdlModel.CurrentProtocol = MethodsByType.Keys.First().CreateObject() as HttpWebClientProtocol;

            if (WsdlModelInitialized != null)
                WsdlModelInitialized(this, EventArgs.Empty);

            Console.WriteLine("Wsdl generation complete.");
        }

        public string WsdlPath
        {
            get { return wsdlPath; }
            set { wsdlPath = value;}
        }

        private void GenerateClassesAndMethods()
        {
            Classes.Clear();
            Methods.Clear();
            MethodsByType.Clear();

            var types = Wsdl.ProxyAssembly.GetTypes().OrderBy(a => a.Name);

            if (types.Count() == 0)
                throw new Exception("Invalid Wsdl.");

            // populate the methods first
            var wsTypes = types.Where(type => typeof(HttpWebClientProtocol).IsAssignableFrom(type));
            List<MethodInfo> allMethods = new List<MethodInfo>();
            foreach (var wsType in wsTypes)
            {
                Console.WriteLine(String.Format("Adding {0}", wsType.Name));
                var methods = wsType.GetMethods().Where(method => method.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), true).Length > 0);
                var methodsList = methods.ToList();
                methodsList.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

                MethodsByType.Add(wsType, methodsList);
                LoadSettings(wsType);

                Methods.AddRange(methodsList);
            }

            // now, only add the classes that are parameters of a method
            Classes.AddRange(
                types.Where(type => !typeof(HttpWebClientProtocol).IsAssignableFrom(type) && GetMethodsByType(type).Any()));
        }

        public List<MethodInfo> GetMethodsByType(Type type)
        {
            var methods =
                   Methods.Where(
                        method => method.ReturnType.FullName == type.FullName || method.ReturnType.FullName == type.FullName + "[]" ||
                                    method.GetParameters().Where(p => p.ParameterType == type).Count() > 0);

            return methods.ToList();
        }

        public static bool IsWebMethod(MethodInfo method)
        {
            object[] customAttributes = method.GetCustomAttributes(typeof(SoapRpcMethodAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                return true;
            }
            customAttributes = method.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                return true;
            }
            customAttributes = method.GetCustomAttributes(typeof(HttpMethodAttribute), true);
            return ((customAttributes != null) && (customAttributes.Length > 0));
        }

        /// <summary>
        /// Main method for a invoking a WebService method.
        /// </summary>
        /// <param name="obj"></param>
        internal void InvokeWebMethod(Events.InvokeEventArgs obj)
        {
            var method = obj.MethodInfo;
            var parameters = obj.Parameters;
            HttpWebClientProtocol protocol = WsdlModel.CurrentProtocol;
            object result = null;
            RequestProperties properties = new RequestProperties(protocol);
            try
            {
                Console.WriteLine(String.Format("InvokeWebMethod {0}.", method.Name));
                Type declaringType = method.DeclaringType;
                if (!protocol.Url.EndsWith(".svc"))
                    WSSWebRequestCreate.RequestTrace = properties;
                                
                result = method.Invoke(protocol, BindingFlags.Public, null, parameters, null);
                obj.Result = result;
                Console.WriteLine(String.Format("InvokeWebMethod {0} successful.", method.Name));
            }
            catch (Exception ex)
            {                
                obj.Result = ex.InnerException != null ? ex.InnerException.ToString() : ex.ToString();
                Console.WriteLine(String.Format("InvokeWebMethod {0} error. See result for details.", method.Name));
            }
            finally
            {
                obj.RequestProperties = properties;
            }

        }

        public static string ProtocolsByNameLocation(Type type)
        {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WebServiceTestStudio",
                    String.Concat(type.Name, "ProtocolsByName.bin"));
        }

        private void LoadSettings(Type wsType)
        {
            var protocolsByName = new ObservableDictionary<string, HttpWebClientProtocol>();
            var protocolsByNameLocation = ProtocolsByNameLocation(wsType);
            var protocolType = wsType;
            if (File.Exists(protocolsByNameLocation))
            {
                try
                {
                    using (var stream = File.OpenRead(protocolsByNameLocation))
                    {
                        BinaryFormatter bFormatter = new BinaryFormatter();
                        var settings = (IDictionary<string, ProtocolSettings>)bFormatter.Deserialize(stream);
                        if (settings != null)
                        {
                            foreach (var kvp in settings)
                            {
                                var defaultProtocol = protocolType.CreateObject() as HttpWebClientProtocol;
                                kvp.Value.PopulateProtocal(defaultProtocol);
                                protocolsByName.Add(kvp.Key, defaultProtocol);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not load saved protocols {0}", ex.Message);
                }
            }

            if (!protocolsByName.Any())
            {
                var defaultProtocol = protocolType.CreateObject() as HttpWebClientProtocol;
                protocolsByName.Add(DefaultProtocolName, defaultProtocol);
            }

            ProtocolSettingsByType.Add(wsType, protocolsByName);
        }

        const string DefaultProtocolName = "Default";

        internal static void Save()
        {
            var protocolSettingsByName = CurrentProtocolsByName.ToDictionary(kvp => kvp.Key, kvp => new ProtocolSettings(kvp.Value));

            var protocolsByNameLocation = ProtocolsByNameLocation(CurrentProtocol.GetType());
            if (!Directory.Exists(Path.GetDirectoryName(protocolsByNameLocation)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(protocolsByNameLocation));
            }

            using (var writer = new FileStream(protocolsByNameLocation, FileMode.Create))
            {
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(writer, protocolSettingsByName);
            }
        }

        [Serializable]
        internal class ProtocolSettings
        {
            public bool AllowAutoRedirect { get; set; }
            public bool EnableDecompression { get; set; }
            public string ConnectionGroupName { get; set; }
            public bool PreAuthenticate { get; set; }
            public int Timeout { get; set; }
            public string Url { get; set; }
            public bool UseDefaultCredentials { get; set; }

            public ProtocolSettings()
            {
            }

            public ProtocolSettings(HttpWebClientProtocol protocol)
            {
                AllowAutoRedirect = protocol.AllowAutoRedirect;
                EnableDecompression = protocol.EnableDecompression;
                ConnectionGroupName = protocol.ConnectionGroupName;
                PreAuthenticate = protocol.PreAuthenticate;
                Timeout = protocol.Timeout;
                Url = protocol.Url;
                UseDefaultCredentials = protocol.UseDefaultCredentials;
            }

            public void PopulateProtocal(HttpWebClientProtocol protocol)
            {
                protocol.AllowAutoRedirect = AllowAutoRedirect;
                protocol.EnableDecompression = EnableDecompression;
                protocol.ConnectionGroupName = ConnectionGroupName;
                protocol.PreAuthenticate = PreAuthenticate;
                protocol.Timeout = Timeout;
                protocol.Url = Url;
                protocol.UseDefaultCredentials = UseDefaultCredentials;
            }
        }
    }
}
