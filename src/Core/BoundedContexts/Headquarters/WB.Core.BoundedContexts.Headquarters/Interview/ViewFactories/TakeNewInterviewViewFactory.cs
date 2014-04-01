using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.BoundedContexts.Headquarters.Interview.Views.TakeNew;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Interview.ViewFactories
{
    public class TakeNewInterviewViewFactory : IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
    {
        private readonly IReadSideRepositoryReader<QuestionnaireDocumentVersioned> surveys;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public TakeNewInterviewViewFactory(IReadSideRepositoryReader<QuestionnaireDocumentVersioned> surveys, IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = this.surveys.GetById(input.QuestionnaireId);

            var view = new TakeNewInterviewView(questionnaire.Questionnaire, questionnaire.Version)
                {
                    Supervisors = users.Query(_ => _.Where(x => x.Roles.Any(role => role == UserRoles.Supervisor))).ToList()
                };

            return view;
        }
    }
}