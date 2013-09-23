using System.Linq;
using Main.Core.Documents;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.TakeNew
{
    public class TakeNewInterviewViewFactory : BaseUserViewFactory, IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
    {
        private readonly IReadSideRepositoryReader<QuestionnaireDocumentVersioned> surveys;

        public TakeNewInterviewViewFactory(IReadSideRepositoryReader<QuestionnaireDocumentVersioned> surveys, IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = this.surveys.GetById(input.QuestionnaireId);

            var view = new TakeNewInterviewView(questionnaire.Questionnaire)
                {
                    Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}