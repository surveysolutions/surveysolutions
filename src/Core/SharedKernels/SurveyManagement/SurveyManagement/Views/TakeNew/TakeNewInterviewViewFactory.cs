using System.Linq;
using Main.Core.Documents;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TakeNew
{
    public class TakeNewInterviewViewFactory : BaseUserViewFactory, IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> surveys;

        public TakeNewInterviewViewFactory(IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> surveys, IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = input.QuestionnaireVersion.HasValue
                ? this.surveys.GetById(input.QuestionnaireId, input.QuestionnaireVersion.Value)
                : this.surveys.GetById(input.QuestionnaireId);

            var view = new TakeNewInterviewView(questionnaire.Questionnaire, questionnaire.Version)
                {
                    Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}