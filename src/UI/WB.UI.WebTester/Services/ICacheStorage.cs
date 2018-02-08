﻿using System;

namespace WB.UI.WebTester.Services
{
    public interface ICacheStorage<TEntity, in TKey> where TEntity: class
    {
        TEntity Get(TKey id, Guid area = default(Guid));

        void Remove(TKey id, Guid area = default(Guid));
        void RemoveArea(Guid area);

        void Store(TEntity entity, TKey id, Guid area = default(Guid));
    }
}