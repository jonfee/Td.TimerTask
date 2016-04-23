using System;
using System.Collections.Generic;
using System.Linq;

namespace KylinService.Services.Queue.Welfare
{
    /// <summary>
    /// 开奖程序
    /// </summary>
    public class LotteryContext
    {
        /// <summary>
        /// 开奖程序－实例化
        /// </summary>
        /// <param name="partCodeArray">参与的编号集合</param>
        /// <param name="lotteryNumber">中奖人数</param>
        public LotteryContext(string[] partCodeArray, int lotteryNumber)
        {
            this._partCodeArray = partCodeArray;
            this._lotteryNumber = lotteryNumber;
        }

        /// <summary>
        /// 参与编号集合
        /// </summary>
        private string[] _partCodeArray;

        /// <summary>
        /// 中奖人数
        /// </summary>
        private int _lotteryNumber;

        /// <summary>
        /// 中奖编号集合
        /// </summary>
        public string[] LotteryResult { get; private set; }

        public void Run()
        {
            if (null != _partCodeArray && _partCodeArray.Length > 0 && _lotteryNumber > 0)
            {
                string[] lotteryCodeArr = null;

                if (_partCodeArray.Length <= _lotteryNumber)
                {
                    lotteryCodeArr = _partCodeArray;
                }
                else
                {
                    lotteryCodeArr = new string[_lotteryNumber];

                    List<string> tempCodes = _partCodeArray.ToList();

                    for (var i = 0; i < _lotteryNumber; i++)
                    {
                        int index = new Random(Guid.NewGuid().GetHashCode()).Next(0, tempCodes.Count());

                        lotteryCodeArr[i] = tempCodes.ElementAt(index);

                        tempCodes.RemoveAt(index);

                        tempCodes.TrimExcess();
                    }
                }

                this.LotteryResult = lotteryCodeArr;
            }
        }//end function
    }
}
