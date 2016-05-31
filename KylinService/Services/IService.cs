using System;

namespace KylinService.Services
{
    /// <summary>
    /// 服务接口
    /// </summary>
    public interface IService : IDisposable
    {
        /// <summary>
        /// 是否已释放
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 服务开始
        /// </summary>
        void Start();

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        void Dispose(bool disposing);
    }
}
