using System;
using System.Collections.Generic;
using DevExpress.RealtorWorld.Xpf.Model;


namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class BanksData : ModuleData {
        IList<MortgageRate> rates;

        public override void Load() {
            base.Load();
            Rates = DataSource.Current.GetMortgageRates();
        }
        public IList<MortgageRate> Rates {
            get { return rates; }
            private set { SetValue<IList<MortgageRate>>("Rates", ref rates, value); }
        }
    }
    public class Banks : ModuleWithNavigator {
        DateTime? savedTimeRangeMinValue;
        DateTime? savedTimeRangeMaxValue;
        DateTime? timeRangeMinValue;
        DateTime? timeRangeMaxValue;

        public Banks() {
            Title = "Mortgage Rates";
        }
        public override void InitData(object parameter) {
            base.InitData(parameter);
            TimeRangeMaxValue = savedTimeRangeMaxValue == null ? new DateTime(2011, 10, 8) : savedTimeRangeMaxValue;
            TimeRangeMinValue = savedTimeRangeMinValue == null ? TimeRangeMaxValue - new TimeSpan(525, 0, 0, 0) : savedTimeRangeMinValue;
        }
        public override void SaveData() {
            base.SaveData();
            savedTimeRangeMaxValue = TimeRangeMaxValue;
            savedTimeRangeMinValue = TimeRangeMinValue;
        }
        public BanksData BanksData { get { return (BanksData)Data; } }
        public DateTime? TimeRangeMinValue {
            get { return timeRangeMinValue; }
            set { SetValue<DateTime?>("TimeRangeMinValue", ref timeRangeMinValue, value); }
        }
        public DateTime? TimeRangeMaxValue {
            get { return timeRangeMaxValue; }
            set { SetValue<DateTime?>("TimeRangeMaxValue", ref timeRangeMaxValue, value); }
        }
    }
}
