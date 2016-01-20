using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeSpan dutTime = DateTime.Now.AddMinutes(-100) - DateTime.Now;

            Tools.CheckPositive(ref dutTime);

            Console.WriteLine(dutTime.Ticks);

            Console.ReadLine();
        }
    }
}
