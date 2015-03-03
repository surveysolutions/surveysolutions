using System;

namespace WB.Core.Infrastructure.ReadSide
{
    /// <summary>
    /// Interface which is implemented by each Raven-specific read side repository writer.
    /// </summary>
    public interface IChacheableRepositoryWriter 
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

        string GetReadableStatus();

        Type ViewType { get; }

        bool IsCacheEnabled { get; }
    }
}