using System.Collections.Generic;
using System.Linq;
using Main.Core.View.Group;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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
        private readonly IReadSideRepositoryReader<QuestionnaireDocument> questionnarieStore;

        public DisplaySurveyViewFactory(IReadSideRepositoryReader<InterviewData> interviewStore,
            IReadSideRepositoryReader<UserDocument> userStore,
            IReadSideRepositoryReader<QuestionnaireDocument> questionnarieStore)
        {
            this.interviewStore = interviewStore;
            this.userStore = userStore;
            this.questionnarieStore = questionnarieStore;
        }

        public SurveyScreenView Load(DisplayViewInputModel input)
        {
            var doc = this.interviewStore.GetById(input.CompleteQuestionnaireId);

            if (doc == null || doc.IsDeleted)
            {
                return null;
            }

            var user = this.userStore.GetById(doc.ResponsibleId);
            var questionnarie = this.questionnarieStore.GetById(doc.QuestionnaireId);

            if (!input.CurrentGroupPublicKey.HasValue)
            {
                input.CurrentGroupPublicKey = doc.InterviewId;
            }
            var screenMenu = BuildScreenMenu(questionnarie);
            var result = new SurveyScreenView();

            result.User = input.User;
            result.Responsible = new UserLight(doc.ResponsibleId, user.UserName);
            result.PublicKey = doc.InterviewId;
            result.Title = questionnarie.Title;
            result.Description = questionnarie.Description;
            result.Status = SurveyStatus.Initial;
            result.Navigation = new ScreenNavigationView(screenMenu, null);
            return result;
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
