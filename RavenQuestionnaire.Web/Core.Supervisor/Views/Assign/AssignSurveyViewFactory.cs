using System.Linq;
using Main.Core.Documents;
using Main.Core.View;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.Views.Assign
{
    public class AssignSurveyViewFactory : BaseUserViewFactory, IViewFactory<AssignSurveyInputModel, AssignSurveyView> 
    {
        private readonly IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> surveys;

        public AssignSurveyViewFactory(IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> surveys, IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        public AssignSurveyView Load(AssignSurveyInputModel input)
        {
            var q = this.surveys.GetById(input.CompleteQuestionnaireId);

            var view = new AssignSurveyView(q)
                {
                    Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}