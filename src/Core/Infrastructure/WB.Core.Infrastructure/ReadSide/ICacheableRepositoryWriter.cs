using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.ReadSide
{
    /// <summary>
    /// Interface which is implemented by each read side repository writer.
    /// </summary>
    public interface ICacheableRepositoryWriter : IReadSideStorage
    {
        /// <summary>
        /// Enables caching of repository entities.
        /// DisableCache method should always be called, because if not, some entities might not be stored to repository.
        /// </summary>
        void EnableCache();

        /// <summary>
        /// Disables caching of repository entities.
        /// Also stores entities which are cached but not yet stored.
        /// </summary>
        void DisableCache();

        bool IsCacheEnabled { get; }
    }
}