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
    public class ROIViewModel : BindableBase
    {
        private ProjectData project;

        public string ProjectName
        {
            get { return this.project.ProjectName; }
        }

        private RoiItem totals;
        public RoiItem Totals
        {
            get { return totals; }
        }

        private ObservableCollection<RoiItem> roiItems;
        public IEnumerable<RoiItem> RoiItems
        {
            get { return roiItems; }

            set
            {
                if (value != null)
                {
                    this.roiItems.Clear();
                    foreach (var item in value)
                    {
                        this.roiItems.Add(item);
                    }

                    OnPropertyChanged("RoiItems");
                }
            }
        }

        public string ROI
        {
            get
            {
                if (this.totals.Investment == 0)
                {
                    return "N/A";
                }
                double roi = (this.totals.Incomes - this.totals.Expenses) / this.totals.Investment; 
                return roi.ToString("P2");
            }
        }

        public string CGR
        {
            get
            {
                if (this.roiItems[0].Investment == 0 || this.project.AnalysisPeriod == 0)
                {
                    return "N/A";
                }

                double beginValue = this.roiItems[0].Investment;
                double endValue = beginValue + this.totals.Incomes
                    - this.totals.Expenses - (this.totals.Investment - beginValue);
                double cgr = Math.Pow(endValue/beginValue, 1.0/this.project.AnalysisPeriod) - 1;
                return cgr.ToString("P2");
            }
        }

        public ROIViewModel()
        {
            this.project = (ProjectData)App.Current.Resources["ProjectData"];
            this.roiItems = new ObservableCollection<RoiItem>();

            this.roiItems.Add(
                new RoiItem( this.project.PeriodUnit + " 0",
                             invest : this.project.TotalInvestment ));

            for (int i = 0; i < this.project.AnalysisPeriod; i++)
            {
                this.roiItems.Add(
                    new RoiItem(this.project.PeriodUnit + " " + (i+1),
                                this.project.TotalExpenses,
                                this.project.TotalIncomes));                
            }

            this.totals = new RoiItem();
            this.calcTotals();
        }

        private void calcTotals()
        {
            double invest = 0;
            double expense = 0;
            double income = 0;

            foreach (var item in this.roiItems)
            {
                invest += item.Investment;
                expense += item.Expenses;
                income += item.Incomes;
            }

            this.totals.Investment = invest;
            this.totals.Expenses = expense;
            this.totals.Incomes = income;
            this.totals.GrossProfit = income - expense;
        }
    }
}
