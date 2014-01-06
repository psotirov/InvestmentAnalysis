using System;
using System.ComponentModel;

namespace Investment_Analysis.Models
{
    public class IrrItem : RoiItem, INotifyPropertyChanged
    {
        private double discountedProfit;
        public double DiscountedProfit
        {
            get { return discountedProfit; }
            set
            {
                discountedProfit = value;
                OnPropertyChanged("DiscountedProfit");
            }
        }

        private double accumulatedCashflow;
        public double AccumulatedCashflow
        {
            get { return accumulatedCashflow; }
            set
            {
                accumulatedCashflow = value;
                OnPropertyChanged("AccumulatedCashflow");
            }
        }

        public IrrItem(string period = "", double expense = 0, double income = 0, double invest = 0)
            : base(period, expense, income, invest)
        {
            this.discountedProfit = income - expense;
            this.accumulatedCashflow = income - expense - invest;
        }
    }
}
