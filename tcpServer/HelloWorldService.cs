using Bindings;
using System;

namespace SelfHost
{
    public class HelloWorldService : IHelloWorldService
    {
        public string SayHello(string name)
        {
            Console.WriteLine($"message Recieved {name}");
            return string.Format("Hello, {0}", name);
        }
    }
}