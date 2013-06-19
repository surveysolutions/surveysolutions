// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq.Expressions;

using Main.DenormalizerStorage;
using System;
using System.Linq;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.View.Questionnaire
{
    using Main.Core.Utility;

    /// <summary>
    /// The questionnaire browse view factory.
    /// </summary>
    public class QuestionnaireBrowseViewFactory : IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document group session.
        /// </summary>
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public QuestionnaireBrowseViewFactory(IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
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
        /// The QuestionnaireBrowseView.
        /// </returns>
        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            // Adjust the model appropriately
            int count = this.documentGroupSession.Count();
            if (count == 0)
            {
                return new QuestionnaireBrowseView(
                    input.Page, input.PageSize, count, new QuestionnaireBrowseItem[0], string.Empty);
            }

            return this.documentGroupSession.Query(queryable =>
            {
                IQueryable<QuestionnaireBrowseItem> query = queryable;

                if (input.IsAdminMode.HasValue)
                {
                    if (input.IsOnlyOwnerItems)
                    {
                        query = query.Where(x => x.CreatedBy == input.CreatedBy);
                    }

                    if (!input.IsAdminMode.Value)
                    {
                        query = query.Where(x => !x.IsDeleted);
                    }

                    if (!string.IsNullOrEmpty(input.Filter))
                    {
                        #warning ReadLayer: ToList materialization because not supported by Raven
                        query = query.ToList().AsQueryable().Where(x => x.Title.ContainsIgnoreCaseSensitive(input.Filter));
                    }
                }

                #warning ReadLayer: ToList materialization because not supported by Raven
                var queryResult = query.ToList().AsQueryable().OrderUsingSortExpression(input.Order);

                var questionnaireItems = queryResult.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();


                return new QuestionnaireBrowseView(input.Page, input.PageSize, queryResult.Count(), questionnaireItems, input.Order);
            });
        }

        #endregion
    }
}