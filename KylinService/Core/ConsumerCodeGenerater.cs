﻿using System;

namespace KylinService.Core
{
    /// <summary>
    /// 消费码生成器
    /// </summary>
    public class ConsumerCodeGenerater
    {
        volatile static ConsumerCodeGenerater _instance = null;

        static readonly object _mylock = new object();

        /// <summary>
        /// 最后一次生成时间
        /// </summary>
        private static DateTime LastGenerateTime;

        /// <summary>
        /// 默认初始标识号
        /// </summary>
        private static long _initTagNo
        {
            get
            {
                return new Random(Guid.NewGuid().GetHashCode()).Next(100001, 199999);
            }
        }

        /// <summary>
        /// 当前标识号
        /// </summary>
        private static long CurrentTagNo;

        private ConsumerCodeGenerater()
        {
            LastGenerateTime = DateTime.Now;
        }

        /// <summary>
        /// 获取消费码
        /// </summary>
        /// <returns></returns>
        public long GetConsumerCode()
        {
            if (LastGenerateTime.Date != DateTime.Now.Date)
            {
                LastGenerateTime = DateTime.Now;

                CurrentTagNo = _initTagNo;
            }

            if (CurrentTagNo < _initTagNo) CurrentTagNo = _initTagNo;

            string code = string.Format("{0}{1}", DateTime.Now.ToString("yyMMdd"), CurrentTagNo);

            CurrentTagNo++;

            return long.Parse(code);
        }

        /// <summary>
        /// 实例对象
        /// </summary>
        public static ConsumerCodeGenerater Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_mylock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConsumerCodeGenerater();
                        }
                    }
                }

                return _instance;
            }
        }//end Instance
    }
}
