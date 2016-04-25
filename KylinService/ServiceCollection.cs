using KylinService.Services;
using System;
using System.Collections;

namespace KylinService
{
    public class ServiceCollection<T> where T : SchedulerService, IService
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
        public T this[object key]
        {
            get
            {
                return hastable[key] as T;
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
        public void Add(object key, T value)
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
                    if (Items != null)
                    {
                        foreach (var item in Items)
                        {
                            if (item is T)
                            {
                                ((T)item).Dispose();
                            }
                        }
                    }
                }

                m_disposed = true;
            }
        }
    }
}
