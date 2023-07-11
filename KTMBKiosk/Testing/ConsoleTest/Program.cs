using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hi.. hello ..");

            Console.WriteLine("wait 5 seconds to end");
            Task.Delay(5000).Wait();

        }
    }
}
