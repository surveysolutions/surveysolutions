using System.Linq;
using Main.Core.Documents;
using Main.Core.View;
using Main.DenormalizerStorage;
using Core.Supervisor.Views.DenormalizerStorageExtensions;

namespace Core.Supervisor.Views.Assign
{
    public class AssignSurveyViewFactory : IViewFactory<AssignSurveyInputModel, AssignSurveyView>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        private readonly IDenormalizerStorage<UserDocument> users;

        public AssignSurveyViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store, IDenormalizerStorage<UserDocument> users)
        {
            this.store = store;
            this.users = users;
        }

        public AssignSurveyView Load(AssignSurveyInputModel input)
        {
            var q = this.store.GetByGuid(input.CompleteQuestionnaireId);

            var view = new AssignSurveyView(q)
                {
                    Supervisors = this.users.GetSupervisorsListForViewer(input.ViewerId).ToList()
                };

            return view;
        }
    }
}