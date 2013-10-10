using System.Collections.Generic;
using System.Linq;
using Main.Core.Utility;
using Main.Core.View;
using Raven.Client.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.BrowseItem
{
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

        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            // Adjust the model appropriately
            int count = this.documentGroupSession.Query(querying => this.FilterQuery(querying, input).Count());
            if (count == 0)
            {
                return new QuestionnaireBrowseView(
                    input.Page, input.PageSize, count, new QuestionnaireBrowseItem[0], string.Empty);
            }

            List<QuestionnaireBrowseItem> questionnaireItems = this.documentGroupSession.Query(querying => questionnaireItems =
                this.FilterQuery(querying, input).OrderUsingSortExpression(input.Order)
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .ToList());

            return new QuestionnaireBrowseView(input.Page, input.PageSize, count, questionnaireItems, input.Order);
        }

        private IQueryable<QuestionnaireBrowseItem> FilterQuery(IQueryable<QuestionnaireBrowseItem> query, QuestionnaireBrowseInputModel input)
        {
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
                    query = query.Where(x => x.Title.StartsWith(input.Filter));
                }
            }
            return query;
        }
    }
}