using System;
using System.Collections;

namespace KylinService.Services
{
    /// <summary>
    /// 任务采集器
    /// </summary>
    public class SchedulerCollection : IDisposable
    {
        Hashtable hastable = Hashtable.Synchronized(new Hashtable());

        static readonly object locking = new object();

        /// <summary>
        /// 数据集合
        /// </summary>
        public ICollection Items
        {
            get
            {
                return hastable.Values;
            }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                return hastable[key];
            }
            set
            {
                hastable[key] = value;
            }
        }

        /// <summary>
        /// 集合数
        /// </summary>
        public int Count
        {
            get { return hastable.Count; }
        }

        /// <summary>
        /// 添加项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(object key, object value)
        {
            lock (locking)
            {
                hastable[key] = value;
            }
        }

        //移除项
        public void Remove(object key)
        {
            lock (locking)
            {
                hastable.Remove(key);
            }
        }

        bool m_disposed;

        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool IsDisposed
        {
            get { return m_disposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    this.Dispose();
                }

                m_disposed = true;
            }
        }
    }
}
