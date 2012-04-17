using System;
using System.Collections.Generic;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class LoanPayment {
        int monthNumber;
        double balance, interest, principal;
        DateTime month;

        public LoanPayment(double balance, double monthlyPayment, int month, double interestRate, DateTime startMonth) {
            this.monthNumber = month;
            this.month = startMonth.AddMonths(month - 1);
            this.interest = Trunc2(balance * interestRate);
            this.principal = Trunc2(monthlyPayment - this.interest);
            this.balance = Trunc2(balance - this.principal);
        }
        public void UpdateBalance() {
            if(this.balance < 0) this.principal += this.balance;
            this.balance = 0;
        }

        public DateTime Month { get { return month; } }
        public int MonthNumber { get { return monthNumber; } }
        public double MonthlyPayment { get { return Interest + Principal; } }
        public double Balance { get { return balance; } }
        public double Interest { get { return interest; } }
        public double Principal { get { return principal; } }
        public static double Trunc2(double val) {
            return Convert.ToDouble(Convert.ToInt64(val * 100)) / 100;
        }
        public static List<LoanPayment> Calculate(double loanAmount, double interestRate, double months, DateTime startMonth, out double payment) {
            payment = (loanAmount * interestRate) / (1 - Math.Exp((-months) * Math.Log(1 + interestRate)));
            payment = LoanPayment.Trunc2(payment);
            List<LoanPayment> payments = new List<LoanPayment>();
            double balance = loanAmount;
            for(int i = 1; i <= months; i++) {
                LoanPayment lp = new LoanPayment(balance, payment, i, interestRate, startMonth);
                balance = lp.Balance;
                payments.Add(lp);
                if(lp.Balance <= 0) break;
            }
            payments[payments.Count - 1].UpdateBalance();
            return payments;
        }
    }
    public class YearPayment {
        public static List<YearPayment> Calculate(List<LoanPayment> payments) {
            List<YearPayment> list = new List<YearPayment>();
            YearPayment summPayment = null;
            List<LoanPayment>.Enumerator enumerator = payments.GetEnumerator();
            while(true) {
                LoanPayment payment = enumerator.MoveNext() ? enumerator.Current : null;
                int year = payment == null ? 0 : payment.Month.Year;
                if(payment == null || summPayment == null || summPayment.Year != year) {
                    if(summPayment != null)
                        list.Add(summPayment);
                    summPayment = payment == null ? null : new YearPayment() { Year = year };
                }
                if(payment == null) break;
                summPayment.Interest += payment.Interest;
                summPayment.Principal += payment.Principal;
            }
            return list;
        }
        public int Year { get; private set; }
        public double Interest { get; set; }
        public double Principal { get; set; }
    }
}
