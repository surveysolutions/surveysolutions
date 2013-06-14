// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportStatisticsFactory.cs" company="">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Core.CAPI.Views.ExporStatistics
{
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ExportStatisticsFactory : IViewFactory<ExporStatisticsInputModel, ExportStatisticsView>
    {
        #region Constants and Fields

        /// <summary>
        /// The store.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportStatisticsFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public ExportStatisticsFactory(IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> store)
        {
            this.store = store;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Loads export statistics view
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// Export statistics view
        /// </returns>
        public ExportStatisticsView Load(ExporStatisticsInputModel input)
        {
            IQueryable<CompleteQuestionnaireBrowseItem> cqs =
                this.store.Query(_ => _.Where(cq => input.Keys.Contains(cq.CompleteQuestionnaireId)).ToList());

            return new ExportStatisticsView(cqs.ToList());
        }

        #endregion
    }
}