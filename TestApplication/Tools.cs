﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    public static class Tools
    {
        /// <summary>
        /// 校正TimeSpan为正数，即小于TimeSpan(0)时，当TimeSpan(0)处理
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static TimeSpan CheckPositive(ref TimeSpan span)
        {
            if (span.Ticks < 0) span = new TimeSpan(0);

            return span;
        }
    }
}
