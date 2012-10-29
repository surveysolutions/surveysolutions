﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExportStatisticsFactory.cs" company="">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> store;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportStatisticsFactory"/> class.
        /// </summary>
        /// <param name="store">
        /// The store.
        /// </param>
        public ExportStatisticsFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> store)
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
                this.store.Query().Where(cq => input.Keys.Contains(cq.CompleteQuestionnaireId));

            return new ExportStatisticsView(cqs.ToList());
        }

        #endregion
    }
}