using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.Views.Summary
{
    using System.Linq;

    using Main.Core.View;
    using Main.Core.View.Questionnaire;

    public class SummaryTemplatesFactory : IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView>
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession;

        public SummaryTemplatesFactory(IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }
        
        public SummaryTemplatesView Load(SummaryTemplatesInputModel input)
        {
            return this.documentGroupSession.Query(
                _ => new SummaryTemplatesView()
                         {
                             Items =
                                 _.OrderBy(x => x.Title)
                                  .ToList()
                                  .Select(
                                      x =>
                                      new SummaryTemplateViewItem()
                                          {
                                              TemplateId =
                                                  x
                                                  .QuestionnaireId,
                                              TemplateName =
                                                  x.Title
                                          })
                         });
        }
    }
}
