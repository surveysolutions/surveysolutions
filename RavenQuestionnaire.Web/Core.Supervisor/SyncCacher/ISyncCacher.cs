﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Supervisor.SyncCacher
{
    public interface ISyncCacher
    {
        void CacheItem(Guid item);

        string GetCachedItem();
    }
}
