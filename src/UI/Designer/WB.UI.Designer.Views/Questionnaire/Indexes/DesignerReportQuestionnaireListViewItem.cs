using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace WB.UI.Designer.Views.Questionnaire.Indexes
{
    public class DesignerReportQuestionnaireListViewItem : AbstractIndexCreationTask<QuestionnaireListViewItem>
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
                                            questionnaire.CreatorName, 
                                            questionnaire.CreatedBy
                                        };

            this.Index(x => x.Title, FieldIndexing.Analyzed);
            this.Index(x => x.CreatorName, FieldIndexing.Analyzed);
        }
    }
}
