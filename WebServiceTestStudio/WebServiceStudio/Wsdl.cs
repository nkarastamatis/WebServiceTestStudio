using System;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.CSharp;

namespace WebServiceStudio
{
    public class Wsdl
    {
        public CSharpCodeProvider CodeProvider { get; set; }
        public CodeCompileUnit CompileUnit { get; set; }
        public StringCollection Paths { get; set; }
        public Assembly ProxyAssembly { get; set; }
        public string ProxyCode { get; set; }
        public StringCollection Wsdls { get; set; }
        public StringCollection Xsds { get; set; }
        public static string[] tempfiles = new string[2];

        public Wsdl()
        {
            Reset();
        }

        ~Wsdl()
        {
            
        }

        private void AddDocument(
            string path, 
            object document, 
            XmlSchemas schemas,
            ServiceDescriptionCollection descriptions)
        {
            var serviceDescription = document as ServiceDescription;
            if (serviceDescription != null)
            {
                if (descriptions[serviceDescription.TargetNamespace] == null)
                {
                    descriptions.Add(serviceDescription);
                    var stringWriter = new StringWriter();
                    var xmlWriter = new XmlTextWriter(stringWriter) {Formatting = Formatting.Indented};
                    serviceDescription.Write(xmlWriter);
                    Wsdls.Add(stringWriter.ToString());
                }
                else
                {
                    // warn of duplicate service
                }
            }
            else
            {
                var schema = document as XmlSchema;
                if (schema != null)
                {
                    if (schemas[schema.TargetNamespace] == null)
                    {
                        schemas.Add(schema);
                        var stringWriter = new StringWriter();
                        var xmlWriter = new XmlTextWriter(stringWriter) {Formatting = Formatting.Indented};
                        schema.Write(xmlWriter);
                        Xsds.Add(stringWriter.ToString());
                    }
                    else
                    {
                        // warn of duplicate schema
                    }
                }
            }
        }

        private static void AddElementAndType(XmlSchema schema, string baseXsdType, string nameSpace)
        {
            //
        }

        private static bool FileExists(string path)
        {
            return !string.IsNullOrEmpty(path) && (path.LastIndexOf('?') == -1) && File.Exists(path);
        }

        private void GetPaths(StringCollection localPaths, StringCollection urls)
        {
            var enumerator = Paths.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (FileExists(current))
                {
                    if (!Path.HasExtension(current))
                        throw new InvalidOperationException(current + " has no extensions");

                    localPaths.Add(current);
                }
                else
                {
                    Uri uri;
                    try
                    {
                        uri = new Uri(current);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException(current + " is not a valid URI");
                    }
                    urls.Add(uri.AbsoluteUri);
                }
            }
        }

        private DiscoveryClientProtocol CreateDiscoveryClient()
        {
            var protocol = new DiscoveryClientProtocol
            {
                AllowAutoRedirect = true,
                Credentials = CredentialCache.DefaultCredentials
            };

            // todo: add proxy & username/password support 

            return protocol;
        }

        public void Generate()
        {
            var descriptions = new ServiceDescriptionCollection();
            var schemas = new XmlSchemas();
            var urls = new StringCollection();
            var localPaths = new StringCollection();

            GetPaths(localPaths, urls);
            
            descriptions.Clear();
            schemas.Clear();

            try
            {
                var client = CreateDiscoveryClient();
                ProcessLocalPaths(client, localPaths, schemas, descriptions);
                //ProcessRemoteUrls(client, urls, schema, descriptions);
            }
            catch (Exception)
            { 
                // warn of error
                return;
            }
            try
            {
                var ext = CodeProvider.FileExtension;
                var userSchemas = new XmlSchemas {schemas};
                foreach (ServiceDescription description in descriptions)
                {
                    userSchemas.Add(description.Types.Schemas);
                }
                var includeSchemas = new Hashtable();
                foreach (XmlSchema userSchema in userSchemas)
                {
                    CollectIncludes(userSchema, includeSchemas);
                }
                Compile(userSchemas);
                GenerateCode(descriptions, schemas, "http://tempuri.org", ext);
                GenerateAssembly();
            }
            catch (Exception)
            {
                return;
            }
        }

        private void GenerateAssembly()
        {
            var assemblyNames = new [] { "System.Xml.dll", "System.dll", "System.Web.Services.dll", "System.Data.dll", Assembly.GetExecutingAssembly().Location };
            var options = new CompilerParameters(assemblyNames) { WarningLevel = 0, GenerateInMemory = false};
            var results = CodeProvider.CompileAssemblyFromSource(options, ProxyCode);
            if (results.Errors.HasErrors)
            {
                foreach (var error in results.Errors)
                {
                    // report error
                }
                throw new Exception("CompilationErrors");
            }
            ProxyAssembly = results.CompiledAssembly;
            //tempfiles[0] = ProxyAssembly.Location;
            //tempfiles[1] = Path.Combine(Path.GetDirectoryName(assemblyNames[4]), Path.GetFileName(ProxyAssembly.Location));
            //System.IO.File.Copy(
            //    tempfiles[0], 
            //    tempfiles[1]);
        }

        private void GenerateCode(
            ServiceDescriptionCollection descriptions, 
            XmlSchemas schemas, 
            string uriToWSDL, 
            string ext)
        {
            ProxyCode = " <ERROR> ";
            StringWriter writer = null;
            CompileUnit = new CodeCompileUnit();
            var importer = new ServiceDescriptionImporter();
            importer.Schemas.Add(schemas);
            foreach (ServiceDescription description in descriptions)
            {
                importer.AddServiceDescription(description, "", "");
            }
            importer.Style = ServiceDescriptionImportStyle.Client;
            importer.ProtocolName = "SOAP"; //todo: support other protocols
            var ns = new CodeNamespace("");
            CompileUnit.Namespaces.Add(ns);
            var warnings = importer.Import(ns, CompileUnit);
            try
            {
                writer = new StringWriter();
                MemoryStream stream = null;
                if (schemas.Count > 0)
                {
                    CompileUnit.ReferencedAssemblies.Add("System.Data.dll");
                    foreach (XmlSchema schema in schemas)
                    {
                        var targetNamespace = schema.TargetNamespace;
                        if (XmlSchemas.IsDataSet(schema))
                        {
                            if (stream == null)
                            {
                                stream = new MemoryStream();
                            }
                            stream.Position = 0L;
                            stream.SetLength(0L);
                            schema.Write(stream);
                            var dataSet = new DataSet();
                            dataSet.ReadXmlSchema(stream);
                            System.Data.Design.TypedDataSetGenerator.Generate(dataSet, ns, CodeProvider);
                        }
                    }
                }

                try
                {
                    GenerateVersionComment(CompileUnit.Namespaces[0]);
                    CodeProvider.GenerateCodeFromCompileUnit(CompileUnit, writer, null);
                }
                catch (Exception exception)
                {
                    writer.Write("Exception in generating code");
                    writer.Write(exception.Message);
                    throw new InvalidOperationException("Error generating ", exception);
                }
            }
            finally
            {
                if (writer != null)
                {
                    ProxyCode = writer.ToString();
                    writer.Close();
                } 
            }
        }

        private static void GenerateVersionComment(CodeNamespace codeNamespace)
        {
            codeNamespace.Comments.Add(new CodeCommentStatement(""));
            var name = Assembly.GetExecutingAssembly().GetName();
            var version = Environment.Version;
            codeNamespace.Comments.Add(new CodeCommentStatement("Assembly " + name.Name + " Version = " + version.ToString()));
            codeNamespace.Comments.Add(new CodeCommentStatement(""));
        }



        private void Compile(XmlSchemas userSchemas)
        {
            var parent = new XmlSchema();
            foreach (XmlSchema userSchema in userSchemas)
            {
                if (userSchema.TargetNamespace != null && userSchema.TargetNamespace.Length == 0)
                {
                    userSchema.TargetNamespace = null;
                }

                if (userSchema.TargetNamespace == parent.TargetNamespace)
                {
                    var item = new XmlSchemaInclude {Schema = userSchema};
                    parent.Includes.Add(item);
                }
                else
                {
                    var import = new XmlSchemaImport {Namespace = userSchema.TargetNamespace, Schema = userSchema};
                    parent.Includes.Add(import);
                }
            }
            AddFakeSchemas(parent, userSchemas);

            try
            {
                var schemas = new XmlSchemaSet();
                schemas.ValidationEventHandler += ValidationCallback;
                schemas.Add(parent);
                if (schemas.Count == 0)
                {
                    //warn of error
                }
            }
            catch (Exception)
            {
                //warn of error
                throw;
            }
        }

        private static void ValidationCallback(object sender, ValidationEventArgs e)
        {
            //warn of error
        }

        private static void AddFakeSchemas(XmlSchema parent, XmlSchemas schemas)
        {
            if (schemas["http://www.w3.org/2001/XMLSchema"] == null)
            {
                var item = new XmlSchemaImport
                {
                    Namespace = "http://www.w3.org/2001/XMLSchema",
                    Schema = CreateFakeXsdSchema("http://www.w3.org/2001/XMLSchema", "schema")
                };
                parent.Includes.Add(item);
            }
            if (schemas["http://schemas.xmlsoap.org/soap/encoding/"] == null)
            {
                var import2 = new XmlSchemaImport
                {
                    Namespace = "http://schemas.xmlsoap.org/soap/encoding/",
                    Schema = CreateFakeSoapEncodingSchema("http://schemas.xmlsoap.org/soap/encoding/", "Array")
                };
                parent.Includes.Add(import2);
            }

            if (schemas["http://schemas.xmlsoap.org/wsdl/"] == null)
            {
                var import3 = new XmlSchemaImport
                {
                    Namespace = "http://schemas.xmlsoap.org/wsdl/",
                    Schema = CreateFakeWsdlSchema("http://schemas.xmlsoap.org/wsdl/")
                };
                parent.Includes.Add(import3);
            }
        }

        private static XmlSchema CreateFakeSoapEncodingSchema(string ns, string name)
        {
            var schema = new XmlSchema {TargetNamespace = ns};
            var item = new XmlSchemaGroup {Name = "Array"};
            var sequence = new XmlSchemaSequence();
            var any = new XmlSchemaAny {MinOccurs = 0M, MaxOccurs = 79228162514264337593543950335M};
            sequence.Items.Add(any);
            any.Namespace = "##any";
            any.ProcessContents = XmlSchemaContentProcessing.Lax;
            item.Particle = sequence;
            schema.Items.Add(item);
            var type = new XmlSchemaComplexType {Name = name};
            var ref2 = new XmlSchemaGroupRef {RefName = new XmlQualifiedName("Array", ns)};
            type.Particle = ref2;
            var attribute = new XmlSchemaAttribute {RefName = new XmlQualifiedName("arrayType", ns)};
            type.Attributes.Add(attribute);
            schema.Items.Add(type);
            attribute = new XmlSchemaAttribute {Use = XmlSchemaUse.None, Name = "arrayType"};
            schema.Items.Add(attribute);
            AddSimpleType(schema, "base64", "base64Binary");
            AddElementAndType(schema, "anyURI", ns);
            AddElementAndType(schema, "base64Binary", ns);
            AddElementAndType(schema, "boolean", ns);
            AddElementAndType(schema, "byte", ns);
            AddElementAndType(schema, "date", ns);
            AddElementAndType(schema, "dateTime", ns);
            AddElementAndType(schema, "decimal", ns);
            AddElementAndType(schema, "double", ns);
            AddElementAndType(schema, "duration", ns);
            AddElementAndType(schema, "ENTITIES", ns);
            AddElementAndType(schema, "ENTITY", ns);
            AddElementAndType(schema, "float", ns);
            AddElementAndType(schema, "gDay", ns);
            AddElementAndType(schema, "gMonth", ns);
            AddElementAndType(schema, "gMonthDay", ns);
            AddElementAndType(schema, "gYear", ns);
            AddElementAndType(schema, "gYearMonth", ns);
            AddElementAndType(schema, "hexBinary", ns);
            AddElementAndType(schema, "ID", ns);
            AddElementAndType(schema, "IDREF", ns);
            AddElementAndType(schema, "IDREFS", ns);
            AddElementAndType(schema, "int", ns);
            AddElementAndType(schema, "integer", ns);
            AddElementAndType(schema, "language", ns);
            AddElementAndType(schema, "long", ns);
            AddElementAndType(schema, "Name", ns);
            AddElementAndType(schema, "NCName", ns);
            AddElementAndType(schema, "negativeInteger", ns);
            AddElementAndType(schema, "NMTOKEN", ns);
            AddElementAndType(schema, "NMTOKENS", ns);
            AddElementAndType(schema, "nonNegativeInteger", ns);
            AddElementAndType(schema, "nonPositiveInteger", ns);
            AddElementAndType(schema, "normalizedString", ns);
            AddElementAndType(schema, "positiveInteger", ns);
            AddElementAndType(schema, "QName", ns);
            AddElementAndType(schema, "short", ns);
            AddElementAndType(schema, "string", ns);
            AddElementAndType(schema, "time", ns);
            AddElementAndType(schema, "token", ns);
            AddElementAndType(schema, "unsignedByte", ns);
            AddElementAndType(schema, "unsignedInt", ns);
            AddElementAndType(schema, "unsignedLong", ns);
            AddElementAndType(schema, "unsignedShort", ns);
            return schema;
        }

        private static XmlSchema CreateFakeWsdlSchema(string ns)
        {
            var schema = new XmlSchema {TargetNamespace = ns};
            var item = new XmlSchemaAttribute
            {
                Use = XmlSchemaUse.None,
                Name = "arrayType",
                SchemaTypeName = new XmlQualifiedName("QName", "http://www.w3.org/2001/XMLSchema")
            };
            schema.Items.Add(item);
            return schema;
        }

        private static XmlSchema CreateFakeXsdSchema(string ns, string name)
        {
            var schema = new XmlSchema {TargetNamespace = ns};
            var item = new XmlSchemaElement {Name = name};
            var type = new XmlSchemaComplexType();
            item.SchemaType = type;
            schema.Items.Add(item);
            return schema;
        }


        private static void AddSimpleType(XmlSchema schema, string typeName, string baseXsdType)
        {
            var item = new XmlSchemaSimpleType {Name = typeName};
            var restriction = new XmlSchemaSimpleTypeRestriction
            {
                BaseTypeName = new XmlQualifiedName(baseXsdType, "http://www.w3.org/2001/XMLSchema")
            };
            item.Content = restriction;
            schema.Items.Add(item);
        }
        
        private static void CollectIncludes(XmlSchema schema, Hashtable includeSchemas)
        {
            var enumerator = schema.Includes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = (XmlSchemaExternal) enumerator.Current;
                if (current == null)
                    break;

                var schemaLocation = current.SchemaLocation;
                if (current is XmlSchemaImport)
                {
                    current.SchemaLocation = null;
                }
                else if ((current is XmlSchemaInclude) && !String.IsNullOrEmpty(schemaLocation))
                {
                    var path = Path.GetFullPath(schemaLocation).ToLower();
                    if (includeSchemas[path] == null)
                    {
                        var readSchema = ReadSchema(schemaLocation);
                        includeSchemas[path] = readSchema;
                        CollectIncludes(readSchema, includeSchemas);
                    }
                    current.Schema = (XmlSchema) includeSchemas[path];
                    current.SchemaLocation = null;
                }
            }
        }

        private static XmlSchema ReadSchema(string schemaLocation)
        {
            var reader = new XmlTextReader(schemaLocation);
            return reader.IsStartElement("schema", "http://www.w3.org/2001/XMLSchema") ? XmlSchema.Read(reader, null) : null;
        }

        private void ProcessLocalPaths(DiscoveryClientProtocol client, StringCollection localPaths, XmlSchemas schemas, ServiceDescriptionCollection descriptions)
        {
            var enumerator = localPaths.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var ext = Path.GetExtension(current);
                object document = null;
                if (string.Compare(ext, ".wsdl", true) == 0)
                {
                    document = ReadLocalDocument(false, current);
                }
                // todo: add support for other file types?

                if (document != null)
                    AddDocument(current, document, schemas, descriptions);
            }
        }

        private static object ReadLocalDocument(bool isSchema, string path)
        {
            object obj = null;
            StreamReader input = null;
            try
            {
                input = new StreamReader(path);
                if (isSchema)
                {
                    obj = XmlSchema.Read(new XmlTextReader(input), null);
                }
                else
                {
                    obj = ServiceDescription.Read(input.BaseStream);
                }
            }
            finally
            {
                if (input != null)
                    input.Close();
            }
            return obj;
        }

        public void Reset()
        {
            CodeProvider = new CSharpCodeProvider();
            Paths = new StringCollection();
            Wsdls = new StringCollection();
            Xsds = new StringCollection();
            CompileUnit = null;
            ProxyAssembly = null;
        }
    }
}
