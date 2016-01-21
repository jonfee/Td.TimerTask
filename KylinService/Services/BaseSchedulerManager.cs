using KylinService.Core;
using System.Collections;
using System.Linq;
using System.Windows.Forms;

namespace KylinService.Services
{
    public abstract class BaseSchedulerManager
    {
        /// <summary>
        /// 务计划列表
        /// </summary>
        public static Hashtable Schedulers = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 校正任务计划列表，将无效的计划移除
        /// </summary>
        /// <param name="keys"></param>
        public virtual void CheckScheduler<T>(T[] keys) where T : struct
        {
            if (null == keys || Schedulers.Keys.Count < 1) return;

            var akeys = new ArrayList(Schedulers.Keys);

            for (int i = 0; i < akeys.Count; i++)
            {
                var id = (T)akeys[i];

                if (!keys.Contains(id))
                {
                    Schedulers.Remove(id);
                }
            }
        }

        /// <summary>
        /// 清空任务计划列表
        /// </summary>
        public virtual void Clear()
        {
            if (null == Schedulers || Schedulers.Keys.Count < 1) return;

            var akeys = new ArrayList(Schedulers.Keys);

            for (int i = 0; i < akeys.Count; i++)
            {
                Schedulers.Remove(akeys[i]);
            }
        }
    }
}
