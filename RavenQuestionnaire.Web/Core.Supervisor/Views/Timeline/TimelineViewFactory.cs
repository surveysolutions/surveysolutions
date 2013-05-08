// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimelineViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire statistic view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Timeline
{
    using Main.Core.Documents;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    /// <summary>
    /// The complete questionnaire statistic view factory.
    /// </summary>
    public class TimelineViewFactory :
        IViewFactory<TimelineViewInputModel, TimelineView>
    {
        #region Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineViewFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public TimelineViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store)
        {
            this.store = store;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Statistics.TimelineView.
        /// </returns>
        public TimelineView Load(TimelineViewInputModel input)
        {
            CompleteQuestionnaireStoreDocument doc = this.store.GetById(input.Id);
            return new TimelineView(doc);
        }

        #endregion
    }
}