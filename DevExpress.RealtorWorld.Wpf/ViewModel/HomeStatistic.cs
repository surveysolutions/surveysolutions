using System;
using System.Collections.Generic;
using DevExpress.RealtorWorld.Xpf.Model;


namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class HomeStatisticData : ModuleData {
        Home home;
        IList<StaticticPoint> popularityRating;
        IList<StaticticPoint> prices;
        IList<SimilarHousesStatisticData> similarHouses;

        public HomeStatisticData(Home home) {
            Home = home;
        }
        public override void Load() {
            base.Load();
            if(Home == null) return;
            PopularityRating = DataSource.Current.GetPopularityRating(Home);
            Prices = DataSource.Current.GetPrices(Home);
            SimilarHouses = DataSource.Current.GetSimilarHouses(Home);
        }
        public Home Home {
            get { return home; }
            private set { SetValue<Home>("Home", ref home, value); }
        }
        public IList<StaticticPoint> PopularityRating {
            get { return popularityRating; }
            private set { SetValue<IList<StaticticPoint>>("PopularityRating", ref popularityRating, value); }
        }
        public IList<StaticticPoint> Prices {
            get { return prices; }
            private set { SetValue<IList<StaticticPoint>>("Prices", ref prices, value); }
        }
        public IList<SimilarHousesStatisticData> SimilarHouses {
            get { return similarHouses; }
            private set { SetValue<IList<SimilarHousesStatisticData>>("SimilarHouses", ref similarHouses, value); }
        }
    }
    public class HomeStatistic : Module {
        static DateTime? savedTimeRangeMinValue;
        static DateTime? savedTimeRangeMaxValue;
        static decimal? savedYearRangeMinValue;
        static decimal? savedYearRangeMaxValue;
        DateTime? timeRangeMinValue;
        DateTime? timeRangeMaxValue;
        decimal yearRangeMinValue;
        decimal yearRangeMaxValue;

        public override void InitData(object parameter) {
            base.InitData(parameter);
            TimeRangeMaxValue = savedTimeRangeMaxValue == null ? DateTime.Now.Date : savedTimeRangeMaxValue;
            TimeRangeMinValue = savedTimeRangeMinValue == null ? TimeRangeMaxValue - new TimeSpan(100, 0, 0, 0) : savedTimeRangeMinValue;
            YearRangeMaxValue = savedYearRangeMaxValue == null ? DateTime.Now.Year + 0.6M : (decimal)savedYearRangeMaxValue;
            YearRangeMinValue = savedYearRangeMinValue == null ? YearRangeMaxValue - 6.2M : (decimal)savedYearRangeMinValue;
        }
        public override void SaveData() {
            base.SaveData();
            savedTimeRangeMaxValue = TimeRangeMaxValue;
            savedTimeRangeMinValue = TimeRangeMinValue;
            savedYearRangeMaxValue = YearRangeMaxValue;
            savedYearRangeMinValue = YearRangeMinValue;
        }
        public HomeStatisticData HomeStatisticData { get { return (HomeStatisticData)Data; } }
        public DateTime? TimeRangeMinValue {
            get { return timeRangeMinValue; }
            set { SetValue<DateTime?>("TimeRangeMinValue", ref timeRangeMinValue, value); }
        }
        public DateTime? TimeRangeMaxValue {
            get { return timeRangeMaxValue; }
            set { SetValue<DateTime?>("TimeRangeMaxValue", ref timeRangeMaxValue, value); }
        }
        public decimal YearRangeMinValue {
            get { return yearRangeMinValue; }
            set { SetValue<decimal>("YearRangeMinValue", ref yearRangeMinValue, value); }
        }
        public decimal YearRangeMaxValue {
            get { return yearRangeMaxValue; }
            set { SetValue<decimal>("YearRangeMaxValue", ref yearRangeMaxValue, value); }
        }
    }
}
