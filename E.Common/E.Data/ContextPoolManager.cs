using System;
using System.Collections;
using System.Collections.Generic;

namespace E.Data
{
    public sealed class ContextPoolManager<TContext> : IDisposable
        where TContext : ContextBase, new()
    {
        private readonly Queue<TContext> _contextPool;
        private readonly int _maxpool;

        internal ContextPoolManager()
        {
            _contextPool = new Queue<TContext>();

            _maxpool = 100;

        }

        internal ContextPoolManager(int maxpool)
        {
            _contextPool = new Queue<TContext>();

            _maxpool = maxpool;

        }

        public TContext Get()
        {
            var pool = _contextPool;
            lock (((ICollection)pool).SyncRoot)
            {
                if (_contextPool.Count > 0)
                {
                    return _contextPool.Dequeue();
                }
                var t = new TContext();
                return t;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public bool End(TContext context)
        {
            var pool = _contextPool;

            lock (((ICollection) pool).SyncRoot)
            {
                if (pool.Count < _maxpool)
                {
                    pool.Enqueue(context);
                    return true;
                }
                context.Dispose();
                return false;
            }
        }

        public void Dispose()
        {
            _contextPool.Clear();
        }
    }
}
