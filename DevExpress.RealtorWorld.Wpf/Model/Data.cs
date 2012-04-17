using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DevExpress.RealtorWorld.Xpf.Model {
    public static class LayoutsData {
        const int LayoutsCount = 5;
        static byte[][] layouts = new byte[LayoutsCount][];

        public static byte[] GetLayout(int homeID) {
            int i = homeID % LayoutsCount;
            if(layouts[i] != null) return layouts[i];
            using(ReusableStream s = DataHelper.GetDataFile(string.Format("Images\\HomePlan{0}.jpg", i + 1))) {
                byte[] data = new byte[s.Data.Length];
                s.Data.Read(data, 0, data.Length);
                layouts[i] = data;
                return layouts[i];
            }
        }
    }
    public static class HomeStatisticsData {
        public static readonly string[] Regions = new string[] { "Middle West", "Mountain", "Pacific", "South", "North East" };

        static Dictionary<int, List<StaticticPoint>> popularity = new Dictionary<int, List<StaticticPoint>>();
        static Dictionary<int, List<StaticticPoint>> prices = new Dictionary<int, List<StaticticPoint>>();
        static Dictionary<int, List<SimilarHousesStatisticData>> similarHouse = new Dictionary<int, List<SimilarHousesStatisticData>>();
        static Random random = new Random();

        public static List<StaticticPoint> GetPopularityRating(int homeID) {
            List<StaticticPoint> statistic;
            if(!popularity.TryGetValue(homeID, out statistic)) {
                statistic = new List<StaticticPoint>();
                foreach(string region in Regions) {
                    statistic.Add(new StaticticPoint() { Argument = region, Value = random.Next(80) });
                }
                popularity.Add(homeID, statistic);
            }
            return statistic;
        }
        public static List<StaticticPoint> GetPrices(int homeID) {
            List<StaticticPoint> statistic;
            if(!prices.TryGetValue(homeID, out statistic)) {
                statistic = new List<StaticticPoint>();
                DateTime beginDate = DateTime.Now;
                DateTime endDate = beginDate - new TimeSpan(500, 0, 0, 0, 0);
                Home home = HomesData.Current.DataSource.Find((h) => { return h.ID == homeID; });
                decimal value = home == null ? 0M : home.Price / 1000M;
                for(DateTime date = beginDate; date > endDate; date = date - new TimeSpan(1, 0, 0, 0, 0)) {
                    statistic.Add(new StaticticPoint() { Argument = date, Value = value });
                    value = value * (decimal)(1 + (random.NextDouble() - 0.5) / 1000);
                }
                prices.Add(homeID, statistic);
            }
            return statistic;
        }
        public static List<SimilarHousesStatisticData> GetSimilarHouses(int homeID) {
            List<SimilarHousesStatisticData> statistic;
            if(!similarHouse.TryGetValue(homeID, out statistic)) {
                statistic = new List<SimilarHousesStatisticData>();
                int year = DateTime.Now.Year;
                for(int i = 10; --i >= 0; ) {
                    SimilarHousesStatisticData data = new SimilarHousesStatisticData();
                    data.Year = year - i;
                    data.ProposalCount = random.Next(50, 250);
                    data.SoldCount = data.ProposalCount * random.Next(10, 80) / 100;
                    statistic.Add(data);
                }
                similarHouse.Add(homeID, statistic);
            }
            return statistic;
        }
    }
    public static class AgentStatisticsData {
        public static readonly string[] Regions = new string[] { "North-East", "Mid-West", "South", "West" };

        static Dictionary<int, List<AgentStatisticData>> statistics = new Dictionary<int, List<AgentStatisticData>>();
        static Random random = new Random();

        public static List<AgentStatisticData> GetAgentStatistic(int agentID) {
            List<AgentStatisticData> ret;
            if(!statistics.TryGetValue(agentID, out ret)) {
                ret = new List<AgentStatisticData>();
                int year = DateTime.Now.Year;
                int baseValue = 0;
                foreach(string region in Regions) {
                    baseValue += 5;
                    for(int i = 0; i < 10; i++) {
                        AgentStatisticData data = new AgentStatisticData() { Region = region };
                        data.Year = year - i;
                        data.Value = random.Next(baseValue);
                        ret.Add(data);
                    }
                }
                statistics.Add(agentID, ret);
            }
            return ret;
        }
    }
    public abstract class ItemsData<CurrentType, DataType> : List<DataType> where CurrentType : new() {
        static List<DataType> dataSource;
        static CurrentType current;

        public static CurrentType Current {
            get {
                if(current == null) {
                    current = new CurrentType();
                }
                return current;
            }
        }
        public abstract List<DataType> NewDataSource { get; }
        public List<DataType> DataSource {
            get {
                if(dataSource == null)
                    dataSource = NewDataSource;
                return dataSource;
            }
        }
    }
    public abstract class XmlItemsData<CurrentType, DataType> : ItemsData<CurrentType, DataType> where CurrentType : new() {
        public override List<DataType> NewDataSource {
            get {
                using(ReusableStream data = GetDataStream()) {
                    XmlSerializer s = new XmlSerializer(typeof(CurrentType));
                    return (List<DataType>)s.Deserialize(data.Data);
                }
            }
        }
        protected abstract ReusableStream GetDataStream();
        protected ReusableStream GetDataStreamCore(string file) {
            return DataHelper.GetDataFile(file);
        }
    }
    [XmlRoot("dsHomes", Namespace = "http://tempuri.org/dsHomes.xsd")]
    public class HomesData : XmlItemsData<HomesData, Home> {
        protected override ReusableStream GetDataStream() {
            return GetDataStreamCore("Homes.xml");
        }
        public static List<Home> GetAgentHomes(int agentID) {
            List<Home> ret = new List<Home>();
            int agentsCount = AgentsSourceData.Current.DataSource.Count;
            foreach(Home home in Current.DataSource) {
                int id = home.ID % agentsCount + 1;
                if(id == agentID)
                    ret.Add(home);
            }
            return ret;
        }
    }
    [XmlRoot("dsPhotos", Namespace = "http://tempuri.org/dsPhotos.xsd")]
    public class HomePhotosData : XmlItemsData<HomePhotosData, HomePhoto> {
        const int PhotosCount = 7;

        public List<HomePhoto> GetPhotos(int homeID) {
            int id = homeID % PhotosCount + 1;
            List<HomePhoto> ret = new List<HomePhoto>();
            foreach(HomePhoto homePhoto in DataSource) {
                if(id == homePhoto.ParentID)
                    ret.Add(homePhoto);
            }
            return ret;
        }
        protected override ReusableStream GetDataStream() {
            return GetDataStreamCore("HomePhotos.xml");
        }
    }
    [XmlRoot("dsHomes", Namespace = "http://tempuri.org/dsHomes.xsd")]
    public class AgentsSourceData : XmlItemsData<AgentsSourceData, Agent> {
        protected override ReusableStream GetDataStream() {
            return GetDataStreamCore("Homes.xml");
        }
        public static Agent GetHomeAgent(int homeID) {
            int id = homeID % Current.DataSource.Count + 1;
            foreach(Agent agent in Current.DataSource)
                if(id == agent.ID) return agent;
            return null;
        }
    }
    [XmlRoot("dsHistoricalMortgageRateData", Namespace = "http://tempuri.org/dsHistoricalMortgageRateData.xsd")]
    public class MortgageRatesData : XmlItemsData<MortgageRatesData, MortgageRate> {
        protected override ReusableStream GetDataStream() {
            return GetDataStreamCore("Mortgage.xml");
        }
    }
    [XmlRoot("dsHousesSales", Namespace = "http://tempuri.org/dsHousesSales.xsd")]
    public class HousesSalesData : XmlItemsData<HousesSalesData, Period> {
        protected override ReusableStream GetDataStream() {
            return GetDataStreamCore("HousesSales.xml");
        }
    }
}
