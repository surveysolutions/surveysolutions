using DevExpress.RealtorWorld.Xpf.ViewModel;

namespace QApp.ViewModel {
    public static class ModulesRegistration {
        public static void RegisterModules() {
            ModulesManager.RegisterModule(typeof(MainScreenData), typeof(MainScreen));
            ModulesManager.RegisterModule(typeof(DraftData), typeof(Draft));
            ModulesManager.RegisterModule(typeof(NavigatorData), typeof(Navigator));
            ModulesManager.RegisterModule(typeof(CompletedQuestionnaireData), typeof(CompletedQuestionnaire));
            ModulesManager.RegisterModule(typeof(CompletedQuestionnairesData), typeof(CompletedQuestionnaires));
            ModulesManager.RegisterModule(typeof(GroupDetailData), typeof(GroupDetail));
            ModulesManager.RegisterModule(typeof(QuestionnaireTemplatesData), typeof(QuestionnaireTemplates));
            ModulesManager.RegisterModule(typeof(QuestionData), typeof(Question));
        }
    }
}
