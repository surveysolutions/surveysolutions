using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TakeNew
{
    public class TakeNewInterviewViewFactory : BaseUserViewFactory, IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> surveys;

        public TakeNewInterviewViewFactory(IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> surveys, IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = input.QuestionnaireVersion.HasValue
                ? this.surveys.AsVersioned().Get(input.QuestionnaireId.FormatGuid(), input.QuestionnaireVersion.Value)
                : this.surveys.GetById(input.QuestionnaireId);

            var view = new TakeNewInterviewView(questionnaire.Questionnaire, questionnaire.Version)
                {
                    Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}