using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using Bindings;
using System.Net.Security;
using System.Diagnostics;

namespace tcpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var ep = new EndpointAddress(Helper.EndPoint);
                var channelFactory = new ChannelFactory<IHelloWorldService>(Helper.Binding(SecurityMode.Transport));
                var channel = channelFactory.CreateChannel(ep);
                while (true)
                {
                    Console.WriteLine("Enter Name or exit");
                    var name = Console.ReadLine();
                    if (name.StartsWith("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }
                    var str = channel.SayHello(name);
                    Console.WriteLine($"{str}");
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

        }
    }
}
