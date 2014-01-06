using Investment_Analysis.Commands;
using Investment_Analysis.Common;
using Investment_Analysis.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Investment_Analysis.ViewModels
{
    public class IRRViewModel : BindableBase
    {
        private ProjectData project;
        
        public string ProjectName { get { return this.project.ProjectName; } }
        public string MeasureUnit { get { return this.project.MeasureUnit; } }
        public string PeriodUnit { get { return this.project.PeriodUnit; } }

        private string irr;
        public string IRR
        {
            get
            {
                if (irr == null)
                {
                    CalcIRR();
                    irr = "wait";
                }

                return irr;
            }
            private set
            {
                this.irr = value;
                OnPropertyChanged("IRR");
            }
        }

        private string npv;
        public string NPV
        {
            get { return npv; }
            private set
            {
                this.npv = value;
                OnPropertyChanged("NPV");
            }
        }

        private string payback;
        public string Payback
        {
            get { return payback; }
            private set
            {
                this.payback = value;
                OnPropertyChanged("Payback");
            }
        }

        private string discountedPayback;
        public string DiscountedPayback
        {
            get { return discountedPayback; }
            private set
            {
                this.discountedPayback = value;
                OnPropertyChanged("DiscountedPayback");
            }
        }

        private ObservableCollection<IrrItem> irrItems;
        public IEnumerable<IrrItem> IrrItems
        {
            get { return irrItems; }

            set
            {
                if (value != null)
                {
                    this.irrItems.Clear();
                    foreach (var item in value)
                    {
                        this.irrItems.Add(item);
                    }

                    this.CalcCollection();
                    this.CalcIRR();
                    OnPropertyChanged("RoiItems");
                }
            }
        }
       
        public IRRViewModel()
        {
            this.project = (ProjectData)App.Current.Resources["ProjectData"];
            this.irrItems = new ObservableCollection<IrrItem>();

            this.irrItems.Add(
                new IrrItem(this.project.PeriodUnit + " 0",
                             invest: this.project.TotalInvestment));

            for (int i = 0; i < this.project.AnalysisPeriod; i++)
            {
                this.irrItems.Add(
                    new IrrItem(this.project.PeriodUnit + " " + (i + 1),
                                this.project.TotalExpenses,
                                this.project.TotalIncomes));
            }

            this.CalcCollection();
            this.CalcIRR();
        }

        private async void CalcIRR()
        {
            var IRRcalc = new IRRCalculation(this.project.GetCashFlowData());
            double irrVal = await IRRcalc.ExecuteAsync();
            if (irrVal > IRRCalculation.MaximumIrrReturnValue)
            {
                this.IRR = "N/A";
            }

            this.IRR = irrVal.ToString("P2");
            this.NPV = IRRcalc.ExecuteNPV(this.project.DiscountRate).ToString("N2");
            this.Payback = IRRcalc.ExecutePayback(0).ToString("N1");
            this.DiscountedPayback = IRRcalc.ExecutePayback(this.project.DiscountRate).ToString("N1");
        }

        private void CalcCollection()
        {
            double accumulated = 0;

            for (int i = 0; i < this.irrItems.Count; i++)
            {
                this.irrItems[i].DiscountedProfit = 
                    this.irrItems[i].GrossProfit / Math.Pow((1 + this.project.DiscountRate), i);
                accumulated += this.irrItems[i].DiscountedProfit - this.irrItems[i].Investment;
                this.irrItems[i].AccumulatedCashflow = accumulated;
            }
        }
    }
}
