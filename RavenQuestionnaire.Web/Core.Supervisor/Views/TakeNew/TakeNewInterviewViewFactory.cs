using System.Linq;
using Core.Supervisor.Views.Assign;
using Main.Core.Documents;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.TakeNew
{
    public class TakeNewInterviewViewFactory : BaseUserViewFactory, IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
    {
        private readonly IReadSideRepositoryReader<QuestionnaireDocument> surveys;

        public TakeNewInterviewViewFactory(IReadSideRepositoryReader<QuestionnaireDocument> surveys, IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = this.surveys.GetById(input.QuestionnaireId);

            var view = new TakeNewInterviewView(questionnaire)
                {
                    Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}