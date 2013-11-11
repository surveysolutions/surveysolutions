using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Indexes
{
    public class DesignerReportQuestionnaireListViewItem : AbstractIndexCreationTask<QuestionnaireListViewItem, QuestionnaireListViewItemSearchable>
    {
        public DesignerReportQuestionnaireListViewItem()
        {
            this.Map = questionnaires => from questionnaire in questionnaires
                select new
                    {
                        questionnaire.PublicId,
                        questionnaire.SharedPersons,
                        questionnaire.Owner,
                        questionnaire.LastEntryDate,
                        questionnaire.IsPublic,
                        questionnaire.IsDeleted,
                        questionnaire.CreationDate,
                        questionnaire.Title,
                        TitleIndexed = questionnaire.Title,
                        questionnaire.CreatorName,
                        questionnaire.CreatedBy
                    };

            TransformResults = (database, questionnaires) =>
                        from questionnaire in questionnaires
                        select new
                        {
                            questionnaire.PublicId,
                            questionnaire.SharedPersons,
                            questionnaire.Owner,
                            questionnaire.LastEntryDate,
                            questionnaire.IsPublic,
                            questionnaire.IsDeleted,
                            questionnaire.CreationDate,
                            questionnaire.Title,
                            TitleIndexed = questionnaire.Title,
                            questionnaire.CreatorName,
                            questionnaire.CreatedBy
                        };

            this.Sort(x => x.Title, SortOptions.String);

          //  Analyzers.Add(x => x.Title, "Raven.Database.Indexing.Collation.Cultures.SvCollationAnalyzer, Raven.Database");
            this.Index(x => x.CreatorName, FieldIndexing.Analyzed);
            this.Index(x => x.TitleIndexed, FieldIndexing.Analyzed);
        }
    }
}
