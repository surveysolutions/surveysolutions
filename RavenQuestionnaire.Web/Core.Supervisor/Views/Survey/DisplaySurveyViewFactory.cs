using WB.Core.Infrastructure;

namespace Core.Supervisor.Views.Survey
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    public class DisplaySurveyViewFactory : IViewFactory<DisplayViewInputModel, SurveyScreenView>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store;

        private readonly ISurveyScreenSupplier surveyScreenSupplier;

        public DisplaySurveyViewFactory(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> store,
                                        ISurveyScreenSupplier surveyScreenSupplier)
        {
            this.store = store;
            this.surveyScreenSupplier = surveyScreenSupplier;
        }

        public SurveyScreenView Load(DisplayViewInputModel input)
        {
            if (input.CompleteQuestionnaireId == Guid.Empty)
            {
                return null;
            }

            var doc = this.store.GetById(input.CompleteQuestionnaireId);

            if (doc == null)
            {
                return null;
            }

            if (!input.CurrentGroupPublicKey.HasValue)
            {
                input.CurrentGroupPublicKey = doc.PublicKey;
            }

            var rout = new ScreenWithRout(doc, input.CurrentGroupPublicKey, input.PropagationKey,
                                          QuestionScope.Supervisor);

            var screenView = new ScreenNavigationView(rout.MenuItems, rout.Navigation);

            var result = this.surveyScreenSupplier.BuildView(doc, rout.Group, screenView);

            result.User = input.User;

            return result;
        }
    }
}
