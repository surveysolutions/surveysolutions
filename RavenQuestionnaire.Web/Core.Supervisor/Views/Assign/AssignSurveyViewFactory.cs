using System.Linq;
using Main.Core.Documents;
using Main.Core.View;
using Main.DenormalizerStorage;

namespace Core.Supervisor.Views.Assign
{
    public class AssignSurveyViewFactory : BaseUserViewFactory, IViewFactory<AssignSurveyInputModel, AssignSurveyView> 
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> _surveys;

        public AssignSurveyViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> surveys, IQueryableDenormalizerStorage<UserDocument> users) : base(users)
        {
            this._surveys = surveys;
            this.users = users;
        }

        public AssignSurveyView Load(AssignSurveyInputModel input)
        {
            var q = this._surveys.GetById(input.CompleteQuestionnaireId);

            var view = new AssignSurveyView(q)
                {
                    Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}