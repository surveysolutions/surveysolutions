using Main.Core.View;
using System;
using System.Linq;
using Main.Core.Utility;
using Raven.Client.Linq;
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

        
        public QuestionnaireListView Load(QuestionnaireListViewInputModel input)
        {
            var count =
                this.documentGroupSession.Query(queryable => this.FilterQuestionnaires(queryable, input).Count());
           var records=
             this.documentGroupSession.Query(queryable =>
            {
                var queryResult =
                    FilterQuestionnaires(queryable, input).OrderUsingSortExpression(input.Order)
                        .Skip((input.Page - 1)*input.PageSize)
                        .Take(input.PageSize)
                        .ToList();
                queryResult.ForEach(x => x.Owner = x.CreatedBy == input.ViewerId ? "me" : x.CreatorName);
                return queryResult;

            });
           return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                  items: records,
                  order: input.Order);
        }

        private IQueryable<QuestionnaireListViewItem> FilterQuestionnaires(IQueryable<QuestionnaireListViewItem> questionnaire,
            QuestionnaireListViewInputModel input)
        {
            var result = questionnaire;
            if (!string.IsNullOrEmpty(input.Filter))
                result = result.Where((x) => 
                  x.Title.StartsWith(  input.Filter)
                    || x.CreatorName.StartsWith(input.Filter));

            if (input.IsAdminMode)
            {
                if (!input.IsPublic)
                    result = result.Where(x => x.CreatedBy == input.ViewerId || x.SharedPersons.Any(person => person == input.ViewerId));
            }
            else
            {
                result = result.Where(x =>!x.IsDeleted);
                if (input.IsPublic)
                    result = result.Where(x => x.IsPublic);
                else
                    result = result.Where(x => x.CreatedBy == input.ViewerId || x.SharedPersons.Any(person => person == input.ViewerId));
            }
            
            return result;
        }
    }
}