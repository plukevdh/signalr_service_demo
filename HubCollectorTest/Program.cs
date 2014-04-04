using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HubCollector;

namespace HubCollectorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var ls = new ListenerService();
            ls.StartListeners();

            Console.ReadKey();
        }
    }
}
