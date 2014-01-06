using System;
using System.ComponentModel;

namespace Investment_Analysis.Models
{
    public class RoiItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Period { get; set; }

        protected double investment;
        public double Investment
        {
            get { return investment; }
            set
            {
                investment = value;
                OnPropertyChanged("Investment");
            }
        }

        protected double expenses;
        public double Expenses
        {
            get { return expenses; }
            set
            {
                expenses = value;
                OnPropertyChanged("Expenses");
            }
        }

        protected double incomes;
        public double Incomes
        {
            get { return incomes; }
            set 
            {
                incomes = value;
                OnPropertyChanged("Incomes");
            }
        }

        protected double grossProfit;
        public double GrossProfit
        {
            get { return grossProfit; }
            set
            {
                grossProfit = value;
                OnPropertyChanged("GrossProfit");
            }
        }

        public RoiItem(string period = "", double expense = 0, double income = 0, double invest = 0)
        {
            this.Period = period;
            this.investment = invest;
            this.expenses = expense;
            this.incomes = income;
            this.grossProfit = income - expense;
        }

        protected void OnPropertyChanged(string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
