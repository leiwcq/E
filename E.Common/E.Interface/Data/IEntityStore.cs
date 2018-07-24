using System;
using System.Collections;
using System.Collections.Generic;

namespace E.Interface.Data
{
    public interface IEntityStore : IDisposable
    {
        T GetById<T>(object id);

        IList<T> GetByIds<T>(ICollection ids);

        T Store<T>(T entity);

        void StoreAll<TEntity>(IEnumerable<TEntity> entities);

        void Delete<T>(T entity);

        void DeleteById<T>(object id);

        void DeleteByIds<T>(ICollection ids);

        void DeleteAll<TEntity>();
    }
}