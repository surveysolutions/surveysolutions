using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class QuestionnaireBrowseViewFactory : IQuestionnaireBrowseViewFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> reader;
        private readonly IWebInterviewConfigProvider interviewConfigProvider;

        public QuestionnaireBrowseViewFactory(
            IPlainStorageAccessor<QuestionnaireBrowseItem> reader,
            IWebInterviewConfigProvider interviewConfigProvider)
        {
            this.reader = reader;
            this.interviewConfigProvider = interviewConfigProvider;
        }

        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            return this.reader.Query(queryable =>
            {
                IQueryable<QuestionnaireBrowseItem> query = queryable.Where(x => !x.IsDeleted);

                if (input.IsAdminMode.HasValue)
                {
                    if (input.IsOnlyOwnerItems)
                    {
                        query = query.Where(x => x.CreatedBy == input.CreatedBy);
                    }
                }

                if (input.QuestionnaireId.HasValue)
                {
                    query = query.Where(x => x.QuestionnaireId == input.QuestionnaireId.Value);
                }

                if ((input.Version ?? 0) > 0)
                {
                    query = query.Where(x => x.Version == input.Version.Value);
                }

                if (!string.IsNullOrEmpty(input.SearchFor))
                {
                    var filterLowerCase = input.SearchFor.ToLower();
                    query = query.Where(x => x.Title.ToLower().Contains(filterLowerCase) || 
                                             (x.Version.ToString().Contains(filterLowerCase) && input.QuestionnaireId != null));
                }

                var queryResult = query.OrderUsingSortExpression(input.Order);

                IQueryable<QuestionnaireBrowseItem> pagedResults = queryResult;

                if (input.PageSize.HasValue)
                {
                    pagedResults = queryResult.Skip((input.Page - 1) * input.PageSize.Value).Take(input.PageSize.Value);
                }

                var itemIds = pagedResults.Select(x => x.Id).ToArray();
                var actualItems = queryable.Where(x => itemIds.Contains(x.Id))
                                           .OrderUsingSortExpression(input.Order)
                                           .Fetch(x => x.FeaturedQuestions)
                                           .ToList();

                foreach (var actualItem in actualItems)
                {
                    var config = interviewConfigProvider.Get(actualItem.Identity());
                    actualItem.WebModeEnabled = config?.Started ?? false;
                }

                return new QuestionnaireBrowseView(input.Page, input.PageSize, queryResult.Count(), actualItems, input.Order);
            });
        }

        public QuestionnaireBrowseItem GetByVariableName(string variable, long version)
        {
            return this.reader.Query(_ => _.FirstOrDefault(x => x.Variable == variable && x.Version == version));
        }

        public QuestionnaireBrowseItem GetById(QuestionnaireIdentity identity)
        {
            return this.reader.GetById(identity.ToString());
        }

        public List<QuestionnaireBrowseItem> GetByIds(params QuestionnaireIdentity[] identities)
        {
            var ids = identities.Select(id => id.ToString()).ToArray();
            return this.reader.Query(x => x.Where(qbi => ids.Contains(qbi.Id))).ToList();
        }

        public IEnumerable<QuestionnaireIdentity> GetAllQuestionnaireIdentities()
            => this.reader.Query(queryable => queryable
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.Id)
                    .ToArray())
                .Select(QuestionnaireIdentity.Parse);

        public QuestionnairesList UniqueQuestionnaireIds(string searchFor, int pageSize)
        {
            return this.reader.Query(queryable =>
            {
                var query = queryable.Where(x => !x.IsDeleted);

                if (!string.IsNullOrEmpty(searchFor))
                {
                    var filterLowerCase = searchFor.ToLower();
                    query = query.Where(x => x.Title.ToLower().Contains(filterLowerCase));
                }

                var ids = query.Select(x => x.QuestionnaireId).Distinct().ToList();

                if (ids.Count == 0)
                    return new QuestionnairesList() { Items = new List<QuestionnaireListItem>(), TotalCount = 0};
                
                var queryItems = queryable.Where(x => !x.IsDeleted);
                
                var groupedItems = queryItems
                    .Where(x => ids.Contains(x.QuestionnaireId))
                    .GroupBy(x => new {x.QuestionnaireId})
                    .ToList();
                    
                var listItems =
                    groupedItems
                    .Select(g => new QuestionnaireListItem
                    {
                        Id = g.Key.QuestionnaireId,
                        Title = g.OrderByDescending(v => v.Version).First().Title
                    })
                    .OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return new QuestionnairesList
                {
                    Items = listItems,
                    TotalCount = listItems.Count
                };
            });
        }
    }
}
