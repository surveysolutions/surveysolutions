using System.Collections.Generic;
using System.Linq;
using Main.Core.View.Group;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Survey
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    public class DisplaySurveyViewFactory : IViewFactory<DisplayViewInputModel, SurveyScreenView>
    {
        private readonly IReadSideRepositoryReader<InterviewData> interviewStore;
        private readonly IReadSideRepositoryReader<UserDocument> userStore;
        private readonly IReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnarieStore;

        public DisplaySurveyViewFactory(IReadSideRepositoryReader<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnarieStore)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.questionnarieStore = questionnarieStore;
        }

        public SurveyScreenView Load(DisplayViewInputModel input)
        {
            var interview = this.interviewStore.GetById(input.CompleteQuestionnaireId);

            if (interview == null || interview.IsDeleted)
            {
                return null;
            }

            var user = this.userStore.GetById(interview.ResponsibleId);
            var questionnarie = this.questionnarieStore.GetById(interview.QuestionnaireId).Questionnaire;

            if (!input.CurrentGroupPublicKey.HasValue)
            {
                input.CurrentGroupPublicKey = interview.InterviewId;
            }
            var screenMenu = BuildScreenMenu(questionnarie);
            var result = new SurveyScreenView();

            result.User = input.User;
            result.Responsible = new UserLight(interview.ResponsibleId, user.UserName);
            result.PublicKey = interview.InterviewId;
            result.Title = questionnarie.Title;
            result.Description = questionnarie.Description;
            result.Status = interview.Status;
            result.Navigation = new ScreenNavigationView(screenMenu, null);
            result.Group = BuildCurrentScreen(questionnarie, interview);
            return result;
        }

        private CompleteGroupMobileView BuildCurrentScreen(QuestionnaireDocument questionnarie, InterviewData interview)
        {
            var screen = new CompleteGroupMobileView()
            {
                PublicKey = interview.InterviewId,
                Title = questionnarie.Title,
                Propagated = questionnarie.Propagated,
                Enabled = true,
                Description = questionnarie.Description,
                QuestionnairePublicKey = interview.InterviewId
            };
            
            return screen;
        }

        private List<DetailsMenuItem> BuildScreenMenu(QuestionnaireDocument questionnarie)
        {
            var menu = new List<DetailsMenuItem>();
            foreach (var group in questionnarie.Children.OfType<IGroup>())
            {
                var menuItem = new DetailsMenuItem();

                menuItem.GroupText = group.Title;
                menuItem.Description = group.Description;
                menuItem.Key = new ScreenKey(group.PublicKey, null, group.Propagated);
                menuItem.Totals=new Counter();
                
                menu.Add(menuItem);
            }

            return menu;
        }
    }
}
