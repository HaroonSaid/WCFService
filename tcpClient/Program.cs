﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net.Security;
using System.Diagnostics;
using CommandLine;
using Service;

namespace tcpClient
{
    public class Options
    {
        [Option('s', "security", Required = false, HelpText = "Security Mode", Default = false)]
        public bool Security { get; set; }
        [Option('h', "host", Required = true, HelpText = "Binding Address", Default = "localhost")]
        public string Host { get; set; }
        [Option('p', "port", Required = true, HelpText = "Binding Address", Default = "443")]
        public int Port { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       var ep = new EndpointAddress($"net.tcp://{o.Host}:{o.Port}/hello");
                       Console.WriteLine($"endPoint:{ep.ToString()}");
                       var mode = o.Security ? SecurityMode.Transport : SecurityMode.None;
                       var channelFactory = new ChannelFactory<IHelloWorldService>(Helper.Binding(mode));
                       var channel = channelFactory.CreateChannel(ep);
                       while (true)
                       {
                           Console.WriteLine("Enter Test String or E to exit");
                           var name = Console.ReadLine();
                           if (name.StartsWith("exit", StringComparison.InvariantCultureIgnoreCase))
                           {
                               break;
                           }
                           var str = channel.SayHello(name);
                           Console.WriteLine($"{str}");
                       }

                   });
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                Console.WriteLine(e.Message);
            }

        }
    }
}
