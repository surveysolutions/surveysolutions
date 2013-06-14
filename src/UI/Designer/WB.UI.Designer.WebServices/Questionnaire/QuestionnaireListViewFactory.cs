// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireListViewFactory.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace WB.UI.Designer.WebServices.Questionnaire
{
    using System.Linq;

    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.Questionnaire;
    using Main.DenormalizerStorage;

    /// <summary>
    ///     The questionnaire browse view factory.
    /// </summary>
    public class QuestionnaireListViewFactory : IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView>
    {
        #region Fields

        /// <summary>
        ///     The document group session.
        /// </summary>
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> document;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireListViewFactory"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        public QuestionnaireListViewFactory(IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> document)
        {
            this.document = document;
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
        public QuestionnaireListView Load(QuestionnaireListViewInputModel input)
        {
            return this.document.Query(queryable =>
            {
                var query = queryable
                    .Where(
                        x =>
                        (input.IsAdmin || x.CreatedBy == input.CreatedBy) && (input.IsAdmin || !x.IsDeleted)
                        && (string.IsNullOrEmpty(input.Filter) || x.Title.ContainsIgnoreCaseSensitive(input.Filter)))
                    .AsQueryable()
                    .OrderUsingSortExpression(input.Order);

                var documentItems =
                    query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);

                return new QuestionnaireListView()
                {
                    Page = input.Page,
                    PageSize = input.PageSize,
                    TotalCount = query.Count(),
                    Items =
                        documentItems.Select(
                            x =>
                            new QuestionnaireListViewItem
                            {
                                Id = x.QuestionnaireId,
                                Title = x.Title
                            }).ToArray(),
                    Order = input.Order
                };
            });
        }

        #endregion
    }
}