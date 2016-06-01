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

            DateTime dd = Convert.ToDateTime("2016-06-01 10:40:00");
            dd = dd.AddSeconds(60);
            if (dd == DateTime.Now)
            {
                Console.WriteLine(DateTime.Now);
            }

            //for(var i = 0; i < 100; i++)
            //{
            //    int code = new Random(Guid.NewGuid().GetHashCode()).Next(0, 2);
            //    Console.WriteLine(code);
            //}

            Console.ReadLine();
        }
    }
}
