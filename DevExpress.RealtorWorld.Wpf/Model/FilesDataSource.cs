using System;
using System.Collections.Generic;

namespace DevExpress.RealtorWorld.Xpf.Model {
    public class FilesDataSource : DataSource {
        public override IList<Home> GetHomes() {
            return HomesData.Current.DataSource;
        }
        public override IList<Agent> GetAgents() {
            return AgentsSourceData.Current.DataSource;
        }
        public override IList<Home> GetAgentHomes(Agent agent) {
            return HomesData.GetAgentHomes(agent.ID);
        }
        public override IList<AgentStatisticData> GetAgentStatistic(Agent agent) {
            return AgentStatisticsData.GetAgentStatistic(agent.ID);
        }
        public override IList<MortgageRate> GetMortgageRates() {
            return MortgageRatesData.Current.DataSource;
        }
        public override IList<HomePhoto> GetPhotos(Home home) {
            return HomePhotosData.Current.GetPhotos(home.ID);
        }
        public override byte[] GetLayout(Home home) {
            return LayoutsData.GetLayout(home.ID);
        }
        public override Agent GetHomeAgent(Home home) {
            return AgentsSourceData.GetHomeAgent(home.ID);
        }
        public override IList<StaticticPoint> GetPopularityRating(Home home) {
            return HomeStatisticsData.GetPopularityRating(home.ID);
        }
        public override IList<StaticticPoint> GetPrices(Home home) {
            return HomeStatisticsData.GetPrices(home.ID);
        }
        public override IList<SimilarHousesStatisticData> GetSimilarHouses(Home home) {
            return HomeStatisticsData.GetSimilarHouses(home.ID);
        }
    }
}
