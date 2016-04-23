using StackExchange.Redis;
using System;
using System.Collections.Generic;
using Td.Kylin.Redis;

namespace Td.Kylin.DataCache.Provider
{
    /// <summary>
    /// 缓存抽象类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CacheItem<T> where T : class, new()
    {
        /// <summary>
        /// Redis缓存配置信息
        /// </summary>
        private CacheConfig _config;

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="itemType"></param>
        protected CacheItem(CacheItemType itemType)
        {
            _config = CacheStartup.RedisConfiguration[itemType];

            if (GetCache() == null) Update();
        }

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="cacheKey"></param>
        protected CacheItem(string cacheKey)
        {
            _config = CacheStartup.RedisConfiguration[cacheKey];
        }

        /// <summary>
        /// 缓存项类型
        /// </summary>
        public CacheItemType ItemType
        {
            get
            {
                return _config != null ? _config.ItemType : default(CacheItemType);
            }
        }

        /// <summary>
        /// 缓存Key
        /// </summary>
        protected string CacheKey
        {
            get
            {
                return _config?.RedisKey;
            }
        }

        /// <summary>
        /// 缓存级别
        /// </summary>
        public CacheLevel Level
        {
            get
            {
                return _config != null ? _config.Level : CacheLevel.Permanent;
            }
        }

        /// <summary>
        /// 当前缓存操作的Redis数据库
        /// </summary>
        protected IDatabase RedisDB
        {
            get
            {
                return RedisManager.Redis.GetDatabase(_config.RedisDbIndex);
            }
        }

        /// <summary>
        /// 更新锁
        /// </summary>
        private readonly static object _updateLock = new object();

        /// <summary>
        /// 是否正在更新
        /// </summary>
        private volatile bool _updating;

        /// <summary>
        /// 一般在更新时使用
        /// </summary>
        private List<T> _tempData;

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        public List<T> Value()
        {
            List<T> _data = null;

            try
            {
                //如果正在更新，则使用更新前的临时数据
                if (_updating)
                {
                    _data = this._tempData;
                }
                else
                {
                    //从缓存中读取
                    _data = GetCache();
                }
            }
            catch (Exception ex)
            {
                //Exception
                _data = ReadDataFromDB();
            }

            return _data;
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <returns></returns>
        protected abstract List<T> GetCache();

        /// <summary>
        /// 设置缓存
        /// </summary>
        protected abstract void SetCache(List<T> data);

        /// <summary>
        /// 更新缓存（从数据库中读取缓存元数据，并记录最后更新缓存的时间）
        /// </summary>
        public void Update()
        {
            lock (_updateLock)
            {
                _updating = true;

                var data = ReadDataFromDB();

                this._tempData = data;
                
                SetCache(data);

                _updating = false;

                this._tempData = null;
            }
        }

        /// <summary>
        /// 从数据库中读取元数据
        /// </summary>
        /// <returns></returns>
        protected abstract List<T> ReadDataFromDB();

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <param name="entity"></param>
        public abstract void Update(T entity);

        /// <summary>
        /// 添加缓存 
        /// </summary>
        /// <param name="entity"></param>
        public abstract void Add(T entity);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="entity"></param>
        public abstract void Delete(T entity);

        /// <summary>
        /// 获取指定字段的数据项
        /// </summary>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public abstract T Get(string hashField);
    }
}
