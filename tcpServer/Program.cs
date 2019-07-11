using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net.Security;
using Bindings;

namespace SelfHost
{

    class Program
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri(Helper.BaseAddress);
            string transport = Helper.UnSecure;
            //var ep = new Uri(Helper.UnSecure);
            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(HelloWorldService), baseAddress))
            {
                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(smb);
                host.AddServiceEndpoint(typeof(IMetadataExchange),
                  MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
                host.AddServiceEndpoint(typeof(IHelloWorldService), 
                    Helper.Binding(SecurityMode.None), Helper.UnSecure);
                //host.AddServiceEndpoint(typeof(IHelloWorldService),
                //    Helper.Binding(SecurityMode.Transport), transport);
                host.Open();
                Console.WriteLine($"The service is ready\n meta:{baseAddress}\n Binding Address:{Helper.UnSecure}");
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
        }
    }
}




