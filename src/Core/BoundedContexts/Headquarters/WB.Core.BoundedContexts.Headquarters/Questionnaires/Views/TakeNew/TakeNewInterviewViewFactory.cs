using Main.Core.Documents;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Views.TakeNew
{
    public class TakeNewInterviewViewFactory : IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
    {
        private readonly IReadSideRepositoryReader<QuestionnaireDocumentVersioned> surveys;

        public TakeNewInterviewViewFactory(IReadSideRepositoryReader<QuestionnaireDocumentVersioned> surveys /*, 
            IQueryableReadSideRepositoryReader<UserDocument> users*/)
            : base(/*users*/)
        {
            this.surveys = surveys;
            //this.users = users;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = this.surveys.GetById(input.QuestionnaireId);

            var view = new TakeNewInterviewView(questionnaire.Questionnaire, questionnaire.Version)
                {
                    //Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}