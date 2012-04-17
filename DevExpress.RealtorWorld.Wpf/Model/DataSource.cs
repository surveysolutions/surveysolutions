using System;
using System.Collections.Generic;


namespace DevExpress.RealtorWorld.Xpf.Model {
    public abstract class DataSource {
        public static DataSource Current { get; set; }
        public abstract IList<Home> GetHomes();
        public abstract IList<Agent> GetAgents();
        public abstract IList<Home> GetAgentHomes(Agent agent);
        public abstract IList<AgentStatisticData> GetAgentStatistic(Agent agent);
        public abstract IList<MortgageRate> GetMortgageRates();
        public abstract IList<HomePhoto> GetPhotos(Home home);
        public abstract byte[] GetLayout(Home home);
        public abstract Agent GetHomeAgent(Home home);
        public abstract IList<StaticticPoint> GetPopularityRating(Home home);
        public abstract IList<StaticticPoint> GetPrices(Home home);
        public abstract IList<SimilarHousesStatisticData> GetSimilarHouses(Home home);
    }
}
