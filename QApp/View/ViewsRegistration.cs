using QApp.ViewModel;
using DevExpress.RealtorWorld.Xpf.View;

namespace QApp.View 
{
    public static class ViewsRegistration 
    {
        public static void RegisterViews() 
        {
            XpfViewsManager viewsManager = new XpfViewsManager();
            viewsManager.RegisterView(typeof(Draft), typeof(DraftView));
            viewsManager.RegisterView(typeof(Question), typeof(QuestionView));
            viewsManager.RegisterView(typeof(Navigator), typeof(NavigatorView));
            viewsManager.RegisterView(typeof(MainScreen), typeof(MainScreenView));
            viewsManager.RegisterView(typeof(GroupDetail), typeof(GroupDetailView));
            viewsManager.RegisterView(typeof(CommonGroupDetail), typeof(CommonGroupDetailView));
            viewsManager.RegisterView(typeof(QuestionnaireDetail), typeof(QuestionnaireDetailView));
            viewsManager.RegisterView(typeof(PropagatedGroupDetail), typeof(PropagatedGroupDetailView));
            viewsManager.RegisterView(typeof(QuestionnaireTemplates), typeof(QuestionnaireTemplatesView));
            viewsManager.RegisterView(typeof(CompletedQuestionnaires), typeof(CompletedQuestionnairesView));
       }
    }
}
