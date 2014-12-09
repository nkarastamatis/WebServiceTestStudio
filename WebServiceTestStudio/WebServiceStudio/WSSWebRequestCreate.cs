using System;
using System.Net;

namespace WebServiceStudio
{
    public class WSSWebRequestCreate : IWebRequestCreate
    {
        private static RequestProperties requestTrace;
        private static object _lock = new object();
        public static System.Threading.Tasks.Task clearTask;

        public virtual WebRequest Create(Uri uri)
        {
            WebRequest webRequest = WebRequest.CreateDefault(uri);
            if (RequestTrace == null)
            {
                return webRequest;
            }

            var request = new WSSWebRequest(webRequest, RequestTrace);
            clearTask.Start();
            return request;
        }

        public static void RegisterPrefixes()
        {
            WSSWebRequestCreate creator = new WSSWebRequestCreate();
            WebRequest.RegisterPrefix("http://", creator);
            WebRequest.RegisterPrefix("https://", creator);
        }

        internal static RequestProperties RequestTrace
        {
            get
            {
                return requestTrace;
            }
            set
            {
                lock(_lock)
                {
                    if (clearTask != null)
                    {
                        if (!clearTask.IsCompleted)
                        {
                            Console.WriteLine("Waiting for previous request to begin.");
                            var stopWatch = new System.Diagnostics.Stopwatch();
                            stopWatch.Start();
                            clearTask.Wait();
                            Console.WriteLine(
                                String.Format(
                                "Waited {0} for previous request to begin.",
                                stopWatch.Elapsed));
                        }
                    }

                    clearTask = new System.Threading.Tasks.Task(() =>
                    {
                        requestTrace = null;
                        Console.WriteLine("requestTrace cleared for next call");
                    });

                    requestTrace = value;
                }
            }
        }
    }
}