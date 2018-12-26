using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class QuestionnaireSearchStorage : IQuestionnaireSearchStorage
    {
        public const string TableName = "questionnairesearchentities";
        public const string TableNameWithSchema = "plainstore." + TableName;

        private readonly IUnitOfWork unitOfWork;

        public QuestionnaireSearchStorage(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
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

            var sql = $"INSERT INTO {TableNameWithSchema} (title, questionnaireid, entityid, entitytype, sectionid, searchtext)" +
                      $"VALUES(@title, @questionnaireId, @entityid, @entityType, @sectionid, to_tsvector(@searchtext)) " +
                      $"ON CONFLICT (questionnaireid, entityid) DO UPDATE " +
                      $"SET questionnaireid = @questionnaireId," +
                      $"    title           = @title," +
                      $"    entityid        = @entityId," +
                      $"    entitytype      = @entityType," +
                      $"    sectionid       = @sectionId," +
                      $"    searchtext      = to_tsvector(@searchText)";

            unitOfWork.Session.Connection.Execute(sql, new
            {
                title = composite.GetTitle(),
                questionnaireId = questionnaireId,
                entityId = composite.PublicKey,
                entityType = GetEntityType(composite),
                sectionId = composite.PublicKey,
                searchText = GetTextUsedForSearch(composite)
            });
        }

        private string GetEntityType(IComposite composite)
        {
            if (composite is IQuestion)
                return ChapterItemType.Question.ToString();
            if (composite is IVariable)
                return ChapterItemType.Variable.ToString();
            if (composite is IStaticText)
                return ChapterItemType.StaticText.ToString();
            if (composite is IGroup)
                return ChapterItemType.Group.ToString();

            throw new ArgumentException("Unsupported entity type: " + composite.GetType().Name);
        }

        private string GetTextUsedForSearch(IComposite composite)
        {
            var textUsedForSearch = composite.GetTitle();
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

        public void Remove(Guid questionnaireId, Guid entityId)
        {
            var sql = $"DELETE from {TableNameWithSchema} s " +
                      $"WHERE s.questionnaireid = :questionnaireId " +
                      $"  AND s.entityid        = :entityId";
            var query = unitOfWork.Session.CreateQuery(sql);
            query.SetParameter("questionnaireId", questionnaireId);
            query.SetParameter("entityId", entityId);
            query.ExecuteUpdate();
        }

        public void RemoveAllEntities(Guid questionnaireId)
        {
            var sql = $"DELETE from {TableNameWithSchema} s " +
                      $"WHERE s.questionnaireid = :questionnaireId ";
            var query = unitOfWork.Session.CreateQuery(sql);
            query.SetParameter("questionnaireId", questionnaireId);
            query.ExecuteUpdate();
        }

        /*
        public string Title { get; set; }
        public string QuestionnaireTitle { get; set; }
        public Guid? FolderId { get; set; }
        public string FolderName { get; set; }
        public Guid QuestionnaireId { get; set; }
        public Guid SectionId { get; set; }
        public Guid EntityId { get; set; }
        public string EntityType { get; set; }
*/

        public SearchResult Search(SearchInput input)
        {
            var order = input.OrderBy ?? "title";

            var sql = $"SELECT * from {TableNameWithSchema} s " +
                      //$"WHERE s.folderid = :folderid " +
                      $"  WHERE searchtext_{TableName}_idx @@ to_tsquery(:query)" +
                      $"ORDER BY :order ASC" +
                      $"LIMIT :pageSize; ";
            var query = unitOfWork.Session.CreateQuery(sql);
            query.SetParameter("query", input.Query);
            query.SetParameter("pageSize", input.PageSize);
            query.SetParameter("order", order);

            var searchResultEntities = query.List<SearchResultEntity>();

            var searchResult = new SearchResult();
            searchResult.Items = searchResultEntities;
            searchResult.TotalCount = searchResultEntities.Count;
            return searchResult;
        }
    }

    public interface IQuestionnaireSearchStorage
    {
        void AddOrUpdateEntity(Guid questionnaireId, IComposite composite);
        void Remove(Guid questionnaireId, Guid entityId);
        SearchResult Search(SearchInput input);
        void RemoveAllEntities(Guid questionnaireId);
    }
}
