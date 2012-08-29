// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEntityRepository.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The EntityRepository interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core
{
    using RavenQuestionnaire.Core.Entities;

    /// <summary>
    /// The EntityRepository interface.
    /// </summary>
    /// <typeparam name="TEntity">
    /// </typeparam>
    /// <typeparam name="TDocument">
    /// </typeparam>
    public interface IEntityRepository<TEntity, TDocument>
        where TEntity : IEntity<TDocument>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        void Add(TEntity entity);

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The TEntity.
        /// </returns>
        TEntity Load(string id);

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        void Remove(TEntity entity);

        #endregion
    }
}