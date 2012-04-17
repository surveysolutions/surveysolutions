using System;
using System.Collections.Generic;
using DevExpress.RealtorWorld.Xpf.Model;


namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class AgentDetailData : ModuleData {
        Agent agent;
        IList<Home> homes;
        IList<AgentStatisticData> salesStatistic;

        public AgentDetailData(Agent agent) {
            Agent = agent;
        }
        public override void Load() {
            base.Load();
            if(Agent == null) return;
            Homes = DataSource.Current.GetAgentHomes(Agent);
            SalesStatistic = DataSource.Current.GetAgentStatistic(Agent);
        }
        public Agent Agent {
            get { return agent; }
            private set { SetValue<Agent>("Agent", ref agent, value); }
        }
        public IList<Home> Homes {
            get { return homes; }
            private set { SetValue<IList<Home>>("Homes", ref homes, value); }
        }
        public IList<AgentStatisticData> SalesStatistic {
            get { return salesStatistic; }
            private set { SetValue<IList<AgentStatisticData>>("SalesStatistic", ref salesStatistic, value); }
        }
    }
    public class AgentDetail : Module {
        static decimal? savedYearRangeMinValue;
        static decimal? savedYearRangeMaxValue;
        decimal yearRangeMinValue;
        decimal yearRangeMaxValue;

        public override void InitData(object parameter) {
            base.InitData(parameter);
            YearRangeMaxValue = savedYearRangeMaxValue == null ? 2011.6M : (decimal)savedYearRangeMaxValue;
            YearRangeMinValue = savedYearRangeMinValue == null ? YearRangeMaxValue - 3.2M : (decimal)savedYearRangeMinValue;
        }
        public override void SaveData() {
            base.SaveData();
            savedYearRangeMaxValue = YearRangeMaxValue;
            savedYearRangeMinValue = YearRangeMinValue;
        }
        public AgentDetailData AgentDetailData { get { return (AgentDetailData)Data; } }
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
