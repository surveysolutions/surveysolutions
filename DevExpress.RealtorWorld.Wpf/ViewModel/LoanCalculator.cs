using System;
using System.Collections.Generic;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.Model;

namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class LoanCalculatorData : ModuleData { }
    public class LoanCalculator : ModuleWithNavigator {
        static decimal? savedLoanAmount;
        static decimal? savedInterestRate;
        static int? savedTermOfLoan;
        static DateTime? savedStartMonth;
        decimal payment;
        decimal loanAmount;
        decimal interestRate;
        List<FormatValue> interestRatesList;
        int termOfLoan;
        List<FormatValue> termOfLoanList;
        DateTime startMonth;
        List<FormatValue> startMonthList;
        List<LoanPayment> payments;
        List<LoanPayment> calculatedPayments;
        decimal calculatedMonthlyPayment;
        List<YearPayment> yearPayments;
        List<YearPayment> calculatedYearPayments;

        public LoanCalculator() {
            Title = "Loan Calculator";
        }
        public override void InitData(object parameter) {
            base.InitData(parameter);
            LoanAmount = savedLoanAmount == null ? 250000M : (decimal)savedLoanAmount;
            InterestRatesList = GetInterestRatesList();
            InterestRate = savedInterestRate == null ? (decimal)InterestRatesList[25].Value : (decimal)savedInterestRate;
            TermOfLoanList = GetTermOfLoanList();
            TermOfLoan = savedTermOfLoan == null ? (int)TermOfLoanList[3].Value : (int)savedTermOfLoan;
            StartMonthList = GetStartMonthList();
            StartMonth = savedStartMonth == null ? (DateTime)StartMonthList[0].Value : (DateTime)savedStartMonth;
            Calculate(false);
        }
        public override void SaveData() {
            base.SaveData();
            savedLoanAmount = LoanAmount;
            savedInterestRate = InterestRate;
            savedTermOfLoan = TermOfLoan;
            savedStartMonth = StartMonth;
        }
        public LoanCalculatorData LoanCalculatorData { get { return (LoanCalculatorData)Data; } }
        public decimal Payment {
            get { return payment; }
            private set { SetValue<decimal>("Payment", ref payment, value); }
        }
        public decimal LoanAmount {
            get { return loanAmount; }
            set { SetValue<decimal>("LoanAmount", ref loanAmount, value); }
        }
        public decimal InterestRate {
            get { return interestRate; }
            set { SetValue<decimal>("InterestRate", ref interestRate, value); }
        }
        public List<FormatValue> InterestRatesList {
            get { return interestRatesList; }
            private set { SetValue<List<FormatValue>>("InterestRatesList", ref interestRatesList, value); }
        }
        public int TermOfLoan {
            get { return termOfLoan; }
            set { SetValue<int>("TermOfLoan", ref termOfLoan, value); }
        }
        public List<FormatValue> TermOfLoanList {
            get { return termOfLoanList; }
            private set { SetValue<List<FormatValue>>("TermOfLoanList", ref termOfLoanList, value); }
        }
        public DateTime StartMonth {
            get { return startMonth; }
            set { SetValue<DateTime>("StartMonth", ref startMonth, value); }
        }
        public List<FormatValue> StartMonthList {
            get { return startMonthList; }
            private set { SetValue<List<FormatValue>>("StartMonthList", ref startMonthList, value); }
        }
        public List<LoanPayment> Payments {
            get { return payments; }
            private set { SetValue<List<LoanPayment>>("Payments", ref payments, value); }
        }
        public List<YearPayment> YearPayments {
            get { return yearPayments; }
            private set { SetValue<List<YearPayment>>("YearPayments", ref yearPayments, value); }
        }
        public void Calculate(bool async) {
            if(async) {
                WaitScreenHelperBase.Current.DoInBackground(BeginCalculate, EndCalculate);
            } else {
                BeginCalculate();
                EndCalculate();
            }
        }
        void BeginCalculate() {
            double monthlyPayment;
            this.calculatedPayments = LoanPayment.Calculate((double)LoanAmount, (double)(InterestRate / 12M), (double)(TermOfLoan * 12), StartMonth, out monthlyPayment);
            this.calculatedMonthlyPayment = (decimal)monthlyPayment;
            this.calculatedYearPayments = YearPayment.Calculate(this.calculatedPayments);
        }
        void EndCalculate() {
            Payments = this.calculatedPayments;
            Payment = this.calculatedMonthlyPayment;
            YearPayments = this.calculatedYearPayments;
            this.calculatedPayments = null;
            this.calculatedYearPayments = null;
        }
        List<FormatValue> GetInterestRatesList() {
            List<FormatValue> interestRatesList = new List<FormatValue>();
            for(decimal interestRate = 0.025M; interestRate < 0.15M; interestRate += 0.00125M)
                interestRatesList.Add(new FormatValue() { Value = interestRate, Text = string.Format("{0:p3}", interestRate) });
            return interestRatesList;
        }
        List<FormatValue> GetStartMonthList() {
            List<FormatValue> startMonthList = new List<FormatValue>();
            DateTime start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            for(int i = 1; i < 7; i++) {
                DateTime month = start.AddMonths(i);
                startMonthList.Add(new FormatValue() { Value = month, Text = string.Format("{0:MMMM, yyyy}", month)});
            }
            return startMonthList;
        }
        List<FormatValue> GetTermOfLoanList() {
            List<FormatValue> list = new List<FormatValue>();
            foreach(int term in new int[] { 1, 5, 10, 15, 20, 25, 30 })
                list.Add(new FormatValue() { Value = term, Text = term.ToString() + (term == 1 ? " year" : " years") });
            return list;
        }
        #region Commands
        protected override void InitializeCommands() {
            base.InitializeCommands();
            CalculateCommand = new SimpleActionCommand(DoCalculate);
        }
        public ICommand CalculateCommand { get; private set; }
        void DoCalculate(object p) { Calculate(true); }
        #endregion
    }
}
