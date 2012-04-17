using System;

namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public static class ModulesRegistration {
        public static void RegisterModules() {
            ModulesManager.RegisterModule(typeof(MainData), typeof(Main));
            ModulesManager.RegisterModule(typeof(NavigatorData), typeof(Navigator));
            ModulesManager.RegisterModule(typeof(ListingData), typeof(Listing));
            ModulesManager.RegisterModule(typeof(HomeDetailData), typeof(HomeDetail));
            ModulesManager.RegisterModule(typeof(AgentsData), typeof(Agents));
            ModulesManager.RegisterModule(typeof(AgentDetailData), typeof(AgentDetail));
            ModulesManager.RegisterModule(typeof(ZillowData), typeof(Zillow));
            ModulesManager.RegisterModule(typeof(BanksData), typeof(Banks));
            ModulesManager.RegisterModule(typeof(DraftData), typeof(Draft));
            ModulesManager.RegisterModule(typeof(StatisticsData), typeof(Statistics));
            ModulesManager.RegisterModule(typeof(HomeStatisticData), typeof(HomeStatistic));
            ModulesManager.RegisterModule(typeof(LoanCalculatorData), typeof(LoanCalculator));
        }
    }
}
