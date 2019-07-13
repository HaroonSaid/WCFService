using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class Helper
    {
        public static string EndPoint => "net.tcp://{0}:{0}/hello";
        public static string UnSecure = "net.tcp://0.0.0.0:51200/hello";
        public static string BaseAddress => "net.tcp://0.0.0.0:51400/hello";
        public static NetTcpBinding Binding(SecurityMode securityMode)

        {

            var binding = new NetTcpBinding(securityMode)
            {
                MaxReceivedMessageSize = int.MaxValue,
                ReaderQuotas =
                                      {
                                          MaxStringContentLength = int.MaxValue,
                                          MaxDepth = int.MaxValue,
                                          MaxArrayLength = int.MaxValue
                                      },
                ReceiveTimeout = TimeSpan.MaxValue,
                SendTimeout = TimeSpan.MaxValue,
                //MaxConnections = 10000,
                //ListenBacklog = 10000,

            };
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            // binding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            return binding;

        }
    }
}
