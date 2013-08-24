using Main.Core.View;
using System;
using System.Linq;
using Main.Core.Utility;

using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.UI.Designer.Views.Questionnaire
{
    /// <summary>
    /// The questionnaire browse view factory.
    /// </summary>
    public class QuestionnaireListViewFactory : IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView>
    {
        #region Fields

        /// <summary>
        /// The document group session.
        /// </summary>
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireListViewItem> documentGroupSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireListViewFactory"/> class.
        /// </summary>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public QuestionnaireListViewFactory(IQueryableReadSideRepositoryReader<QuestionnaireListViewItem> documentGroupSession)
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
        public QuestionnaireListView Load(QuestionnaireListViewInputModel input)
        {
            Func<QuestionnaireListViewItem, bool> q =
                (x) =>
                string.IsNullOrEmpty(input.Filter)
                || (x.Title.ContainsIgnoreCaseSensitive(input.Filter)
                    || x.CreatorName.ContainsIgnoreCaseSensitive(input.Filter));
             

            if (input.IsAdminMode)
            {
                q = q.AndAlso(x => (input.IsPublic || (x.CreatedBy == input.ViewerId || 
                                    x.SharedPersons.Contains(input.ViewerId))));
            }
            else
            {
                q =
                    q.AndAlso(
                        x =>
                        !x.IsDeleted && (((x.CreatedBy == input.ViewerId || x.SharedPersons.Contains(input.ViewerId)) && !input.IsPublic) || 
                        (input.IsPublic && x.IsPublic)));
            }


            return this.documentGroupSession.Query(queryable =>
            {
                var queryResult = queryable.Where(q).AsQueryable();
                queryResult.ToList().ForEach(x => x.Owner = x.CreatedBy == input.ViewerId ? "owner" : x.CreatorName);

                return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: queryResult.Count(),
                    items: queryResult.OrderUsingSortExpression(input.Order).Skip((input.Page - 1)*input.PageSize).Take(input.PageSize),
                    order: input.Order);
            });
        }

        #endregion
    }
}