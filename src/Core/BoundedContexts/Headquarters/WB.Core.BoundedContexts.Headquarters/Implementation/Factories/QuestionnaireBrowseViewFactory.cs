﻿using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class QuestionnaireBrowseViewFactory : IQuestionnaireBrowseViewFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> reader;

        public QuestionnaireBrowseViewFactory(IPlainStorageAccessor<QuestionnaireBrowseItem> reader)
        {
            this.reader = reader;
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

                    if (input.QuestionnaireId.HasValue)
                    {
                        query = query.Where(x => x.QuestionnaireId == input.QuestionnaireId.Value);
                    }

                    if (input.Version.HasValue)
                    {
                        query = query.Where(x => x.Version == input.Version.Value);
                    }

                }

                if (!string.IsNullOrEmpty(input.SearchFor))
                {
                    var filterLowerCase = input.SearchFor.ToLower();
                    query = query.Where(x => x.Title.ToLower().Contains(filterLowerCase));
                }

                if (input.OnlyCensus.HasValue)
                {
                    query = query.Where(x => x.AllowCensusMode == input.OnlyCensus);
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

                return new QuestionnaireBrowseView(input.Page, input.PageSize, queryResult.Count(), actualItems, input.Order);
            });
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
                var query = queryable.Where(x => !x.IsDeleted)
                    .Select(x => new {x.QuestionnaireId, x.Title});

                if (!string.IsNullOrEmpty(searchFor))
                {
                    var filterLowerCase = searchFor.ToLower();
                    query = query.Where(x => x.Title.ToLower().Contains(filterLowerCase));
                }

                var list = query.ToList();
                var questionnaireListItems = list.Distinct()
                    .Select(x => new QuestionnaireListItem
                    {
                        Id = x.QuestionnaireId,
                        Title = x.Title
                    });

                var listItems = questionnaireListItems.ToList();
                return new QuestionnairesList
                {
                    Items = listItems,
                    TotalCount = listItems.Count
                };
            });
        }
    }
}
