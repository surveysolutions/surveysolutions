using System.Linq;
using Main.Core.Documents;
using Main.Core.View;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Assign
{
    public class AssignSurveyViewFactory : BaseUserViewFactory, IViewFactory<AssignSurveyInputModel, AssignSurveyView> 
    {
        private readonly IReadSideRepositoryReader<QuestionnaireBrowseItem> templates;

        public AssignSurveyViewFactory(IReadSideRepositoryReader<QuestionnaireBrowseItem> templates, IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.templates = templates;
            this.users = users;
        }

        public AssignSurveyView Load(AssignSurveyInputModel input)
        {
            var q = this.templates.GetById(input.CompleteQuestionnaireId);

            var view = new AssignSurveyView(q, input.CompleteQuestionnaireId)
                {
                    Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}