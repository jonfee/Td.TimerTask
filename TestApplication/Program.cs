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
            //TimeSpan dutTime = DateTime.Now.AddMinutes(-100) - DateTime.Now;

            //Tools.CheckPositive(ref dutTime);

            //Console.WriteLine(dutTime.Ticks);

           

            for(var i = 0; i < 100; i++)
            {
                int code = new Random(Guid.NewGuid().GetHashCode()).Next(0, 2);
                Console.WriteLine(code);
            }

            Console.ReadLine();
        }
    }
}
