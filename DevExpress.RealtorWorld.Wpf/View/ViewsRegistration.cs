using System;
using DevExpress.RealtorWorld.Xpf.ViewModel;

namespace DevExpress.RealtorWorld.Xpf.View {
    public static class ViewsRegistration {
        public static void RegisterViews() {
            XpfViewsManager viewsManager = new XpfViewsManager();
            viewsManager.RegisterView(typeof(Main), typeof(MainView));
            viewsManager.RegisterView(typeof(Navigator), typeof(NavigatorView));
            viewsManager.RegisterView(typeof(Listing), typeof(ListingView));
            viewsManager.RegisterView(typeof(Agents), typeof(AgentsView));
            viewsManager.RegisterView(typeof(HomeDetail), typeof(HomeDetailView));
            viewsManager.RegisterView(typeof(AgentDetail), typeof(AgentDetailView));
            viewsManager.RegisterView(typeof(Zillow), typeof(ZillowView));
            viewsManager.RegisterView(typeof(Banks), typeof(BanksView));
            viewsManager.RegisterView(typeof(Draft), typeof(DraftView));
            viewsManager.RegisterView(typeof(Statistics), typeof(StatisticsView));
            viewsManager.RegisterView(typeof(HomeStatistic), typeof(HomeStatisticView));
            viewsManager.RegisterView(typeof(LoanCalculator), typeof(LoanCalculatorView));
        }
    }
}
