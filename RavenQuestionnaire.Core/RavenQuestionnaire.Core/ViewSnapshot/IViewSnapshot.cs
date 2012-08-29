// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewSnapshot.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The ViewSnapshot interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.ViewSnapshot
{
    using System;

    using Ncqrs;
    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The ViewSnapshot interface.
    /// </summary>
    public interface IViewSnapshot
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// Type T.
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        T GetByGuid<T>(Guid key) where T : class;

        #endregion
    }

    /// <summary>
    /// The default view snapshot.
    /// </summary>
    public class DefaultViewSnapshot : IViewSnapshot
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly ISnapshotStore store;

        #endregion

        //// private readonly ICommandService commandService;
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewSnapshot"/> class.
        /// </summary>
        public DefaultViewSnapshot()
        {
            this.store = NcqrsEnvironment.Get<ISnapshotStore>();

            //// this.commandService = NcqrsEnvironment.Get<ICommandService>();
            //// NcqrsEnvironment.Get<IUnitOfWorkFactory>().CreateUnitOfWork(Guid.NewGuid()).GetById<>()
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// Type T.
        /// </typeparam>
        /// <returns>
        /// The T.
        /// </returns>
        public T GetByGuid<T>(Guid key) where T : class
        {
            Snapshot snapshot = this.store.GetSnapshot(key, int.MaxValue);
            if (snapshot == null)
            {
                return null;

                /*if(typeof(T) == typeof(CompleteQuestionnaireDocument) )
                {
                    commandService.Execute(new PreLoadCompleteQuestionnaireCommand() { CompleteQuestionnaireId = key});
                }
                else*/

                /*if (typeof(T) == typeof(QuestionnaireDocument))
                {
                    commandService.Execute(new PreLoadQuestionnaireCommand() { QuestionnaireId = key });
                }*/
                /*
                snapshot = this.store.GetSnapshot(key, int.MaxValue);
                if(snapshot == null)
                    return null;*/
            }

            return snapshot.Payload as T;
        }

        #endregion
    }
}