using System;
using Td.Kylin.EnumLibrary;

namespace KylinService.Core
{
    public class IDCreater
    {
        volatile static IDCreater _instance = null;

        static readonly object _mylock = new object();

        /// <summary>
        /// 最后一次生成时间
        /// </summary>
        private static DateTime LastGenerateTime;

        /// <summary>
        /// 默认初始标识号
        /// </summary>
        private static long _initTagNo = 102030405060;

        /// <summary>
        /// 当前标识号
        /// </summary>
        private static long CurrentTagNo;

        private IDCreater()
        {
            LastGenerateTime = DateTime.Now;
        }

        /// <summary>
        /// 获取ID
        /// </summary>
        /// <returns></returns>
        public long GetID()
        {
            if (LastGenerateTime.Date != DateTime.Now.Date)
            {
                LastGenerateTime = DateTime.Now;

                CurrentTagNo = _initTagNo;
            }

            if (CurrentTagNo < _initTagNo) CurrentTagNo = _initTagNo;

            string code = string.Format("8{0}{1}", DateTime.Now.ToString("yyMMdd"), CurrentTagNo);

            CurrentTagNo++;

            return long.Parse(code);
        }

        /// <summary>
        /// 获取基础流水号（23位）
        /// </summary>
        /// <returns></returns>
        public string GetPlatformTransactionCode(PlatformTransactionType transType,int areaID)
        {
            string date = DateTime.Now.ToString("yyMMdd");
            string trans = transType.ToString("d").PadLeft(2, '0');
            string area = areaID.ToString().PadLeft(6, '0');
            string rand = new Random(Guid.NewGuid().GetHashCode()).Next(100000000, 999999999).ToString();

            return string.Format("{0}{1}{2}{3}", date, trans, area, rand);
        }

        /// <summary>
        /// 实例对象
        /// </summary>
        public static IDCreater Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_mylock)
                    {
                        if (_instance == null)
                        {
                            _instance = new IDCreater();
                        }
                    }
                }

                return _instance;
            }
        }//end Instance
    }
}
