using System.Collections.Generic;

namespace WB.Core.Infrastructure.EventHandlers
{
    public class EntitiesState<TEntity>
    {
        public List<TEntity> AddedOrUpdated { get; set; } = new List<TEntity>();
        public List<TEntity> Removed { get; set; } = new List<TEntity>();
    }
}