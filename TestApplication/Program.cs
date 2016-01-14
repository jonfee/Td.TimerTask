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
            List<DateTime> OnTimes = new List<DateTime>();

            OnTimes.Add(new DateTime(4, 1, 11, 13, 30, 30));
            OnTimes.Add(new DateTime(4, 1, 12, 13, 30, 30));
            OnTimes.Add(new DateTime(4, 2, 11, 13, 30, 30));
            OnTimes.Add(new DateTime(4, 2, 12, 13, 30, 30));
            OnTimes.Add(new DateTime(4, 1, 12, 13, 30, 30));
            OnTimes.Add(new DateTime(4, 2, 11, 13, 30, 30));

            OnTimes = Td.Task.Framework.TimerStrategyManager.SortAndDistinct(OnTimes);
            OnTimes.TrimExcess();

            foreach(var item in OnTimes)
            {
                Console.WriteLine(item.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            Console.ReadLine();
        }
    }
}
