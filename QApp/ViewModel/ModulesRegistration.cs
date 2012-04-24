using DevExpress.RealtorWorld.Xpf.ViewModel;

namespace QApp.ViewModel {
    public static class ModulesRegistration {
        public static void RegisterModules() {
            ModulesManager.RegisterModule(typeof(MainScreenData), typeof(MainScreen));
            ModulesManager.RegisterModule(typeof(DraftData), typeof(Draft));
            ModulesManager.RegisterModule(typeof(NavigatorData), typeof(Navigator));
            ModulesManager.RegisterModule(typeof(CompletedQuestionnairesData), typeof(CompletedQuestionnaires));
            ModulesManager.RegisterModule(typeof(GroupDetailData), typeof(GroupDetail));
            ModulesManager.RegisterModule(typeof(QuestionnaireTemplatesData), typeof(QuestionnaireTemplates));
            ModulesManager.RegisterModule(typeof(QuestionData), typeof(Question));

            ModulesManager.RegisterModule(typeof(CommonGroupDetailData), typeof(CommonGroupDetail));

            ModulesManager.RegisterModule(typeof(QuestionnaireDetailData), typeof(QuestionnaireDetail));

            ModulesManager.RegisterModule(typeof(PropagatedGroupDetailData), typeof(PropagatedGroupDetail));
       }
    }
}
