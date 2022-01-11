using System;
using System.Linq;
using Dapper;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class QuestionnaireSearchStorage : IQuestionnaireSearchStorage
    {
        private readonly DesignerDbContext dbContext;
        public const string TableName = "questionnairesearchentities";
        public const string TableNameWithSchema = "plainstore." + TableName;


        public QuestionnaireSearchStorage(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddOrUpdateEntity(Guid questionnaireId, IComposite composite)
        {
            if (!(composite is IQuestion)
                && !(composite is IVariable)
                && !(composite is IGroup)
                && !(composite is IStaticText))
            {
                throw new ArgumentException("AddOrUpdateEntity type is not supported " + composite.GetType());
            }

            var sql = $"INSERT INTO {TableNameWithSchema} (title, questionnaireid, entityid, entitytype, searchtext)" +
                      $"VALUES(@title, @questionnaireId, @entityid, @entityType, to_tsvector(@searchtext)) " +
                      $"ON CONFLICT (questionnaireid, entityid) DO UPDATE " +
                      $"SET questionnaireid = @questionnaireId," +
                      $"    title           = @title," +
                      $"    entityid        = @entityId," +
                      $"    entitytype      = @entityType," +
                      $"    searchtext      = to_tsvector(@searchText)";

            dbContext.Database.GetDbConnection().Execute(sql, new
            {
                title = GetTitle(composite),
                questionnaireId = questionnaireId,
                entityId = composite.PublicKey,
                entityType = GetEntityType(composite),
                searchText = GetTextUsedForSearch(composite)
            });
        }

        private string GetEntityType(IComposite composite)
        {
            if (composite is IQuestion question)
                return question.QuestionType.ToString(); //ChapterItemType.Question.ToString();
            if (composite is IVariable)
                return ChapterItemType.Variable.ToString();
            if (composite is IStaticText)
                return ChapterItemType.StaticText.ToString();

            if (composite is IGroup group)
            {
                return group.IsRoster
                    ? ChapterItemType.Roster.ToString()
                    : ChapterItemType.Group.ToString();
            }

            throw new ArgumentException("Unsupported entity type: " + composite.GetType().Name);
        }

        private string? GetTextUsedForSearch(IComposite composite)
        {
            var textUsedForSearch = GetTitle(composite);
            if (composite is IQuestion question)
            {
                if (question.QuestionType == QuestionType.SingleOption
                    || question.QuestionType == QuestionType.MultyOption)
                {
                    question.Answers.Aggregate(textUsedForSearch, (text, answer)  => text + Environment.NewLine + answer.AnswerText);
                }
            }
            return textUsedForSearch;
        }

        public static string? GetTitle(IQuestionnaireEntity entity)
            => (entity as IQuestion)?.QuestionText
            ?? (entity as IStaticText)?.Text
            ?? (entity as IGroup)?.Title
            ?? (entity as IVariable)?.Label;


        public void Remove(Guid questionnaireId, Guid entityId)
        {
            var sql = $"DELETE from {TableNameWithSchema} s " +
                      $"WHERE s.questionnaireid = @questionnaireId " +
                      $"  AND s.entityid        = @entityId";

            dbContext.Database.GetDbConnection().Execute(sql, new
            {
                questionnaireId = questionnaireId,
                entityId = entityId,
            });
        }

        public void RemoveAllEntities(Guid questionnaireId)
        {
            var sql = $"DELETE from {TableNameWithSchema} s " +
                      $"WHERE s.questionnaireid = @questionnaireId ";
            dbContext.Database.GetDbConnection().Execute(sql, new
            {
                questionnaireId = questionnaireId,
            });
        }

        public SearchResult Search(SearchInput input)
        {
            var textSearchQuery = CreateTextSearchQuery(input.Query);

            var sqlSelect = $"SELECT s.title, s.questionnaireid, s.entityid, s.entitytype, " +
                      $"       li.folderid, li.title as questionnairetitle, f.title as foldername, " +
                      $"       ts_rank_cd(s.searchtext, to_tsquery(@query)) AS rank" +
                      $" FROM {TableNameWithSchema} s " +
                      $"    INNER JOIN plainstore.questionnairelistviewitems li ON s.questionnaireid = li.publicid" +
                      $"     LEFT JOIN plainstore.questionnairelistviewfolders f ON f.id = li.folderid" +
                      $" WHERE (@query IS NULL OR s.searchtext @@ to_tsquery(@query))" +
                      $"   AND (@folderid IS NULL OR li.folderid = @folderid OR f.path like @folderpathquery) " +
                      $" ORDER BY rank DESC" +
                      $" LIMIT @pageSize" +
                      $" OFFSET @offset ";

            var searchResultEntities = dbContext.Database.GetDbConnection().Query<SearchResultEntity>(sqlSelect, new
            {
                query = textSearchQuery,
                folderid = input.FolderId,
                folderpathquery = "%" +input.FolderId + "%",
                pageSize = input.PageSize,
                offset = input.PageIndex * input.PageSize
            }).ToList();

            var sqlCount = $"SELECT COUNT(s.entityid) " +
                      $" FROM {TableNameWithSchema} s " +
                      $"    INNER JOIN plainstore.questionnairelistviewitems li ON s.questionnaireid = li.publicid" +
                      $"     LEFT JOIN plainstore.questionnairelistviewfolders f ON f.id = li.folderid" +
                      $" WHERE (@query IS NULL OR s.searchtext @@ to_tsquery(@query))" +
                      $"   AND (@folderid IS NULL OR li.folderid = @folderid OR f.path like @folderpathquery) ";
            var count = dbContext.Database.GetDbConnection().ExecuteScalar<int>(sqlCount, new
            {
                query = textSearchQuery,
                folderid = input.FolderId,
                folderpathquery = "%" + input.FolderId + "%",
            });

            var searchResult = new SearchResult();
            searchResult.Items = searchResultEntities;
            searchResult.TotalCount = count;
            return searchResult;
        }

        private string? CreateTextSearchQuery(string? inputQuery)
        {
            inputQuery = inputQuery?.Trim().ToLower();
            if (string.IsNullOrEmpty(inputQuery))
                return inputQuery;

            var words = inputQuery.Split(' ');
            if (words.Length == 1)
            {
                return $"{words[0]}:*"; // search word as like
            }

            return string.Join(" & ", words); // use logic AND
        }
    }
}
