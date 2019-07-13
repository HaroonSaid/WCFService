using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net.Security;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using Service;
using Amazon.CertificateManager;
using System.Threading.Tasks;

namespace SelfHost
{

    class Program
    {
        static readonly string certArn = "arn:aws:acm:us-west-2:971217900852:certificate/6ddf0993-68ac-4cd7-bf2c-979a338371cb";

        static async Task Main(string[] args)
        {
            Uri baseAddress = new Uri(Helper.BaseAddress);
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
                    Helper.Binding(SecurityMode.Transport), Helper.UnSecure);
                var amazonCertificateManager = new AmazonCertificateManagerClient(Amazon.RegionEndpoint.USWest2);
                var certManager = new CertifcateManager.AcmClient(amazonCertificateManager);
                var cert = await certManager.GetCertificate(certArn);

                //var cert = Helper.GetCertificate();
                host.Credentials.ServiceCertificate.Certificate = cert;
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;


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




