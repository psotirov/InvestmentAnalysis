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

namespace Investment_Analysis.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        private ProjectData project;
        private ICommand newProject;
        private ICommand openProject;

        // TODO: Add valdation rules to each field, where it is аpropriаtе
        public string ProjectName
        {
            get { return this.project.ProjectName; }
            set { this.project.ProjectName = value; }
        }

        public string MeasureUnit
        {
            get { return this.project.MeasureUnit; }
            set { this.project.MeasureUnit = value; }
        }

        public string TotalInvestment
        {
            get { return this.project.TotalInvestment.ToString("N2"); }
            set
            {
                if (!IsReadOnlyInvestment)
                {
                    double val;
                    double.TryParse(value, out val);
                    this.project.TotalInvestment = val;
                    OnPropertyChanged(string.Empty);
                    this.CalculateIRR();
                }
            }
        }

        public bool IsReadOnlyInvestment
        { 
            get { return (this.project.InvestmentItems != null && 
                          this.project.InvestmentItems.Count > 0); }
        }

        public string TotalExpenses
        {
            get { return this.project.TotalExpenses.ToString("N2"); }
            set
            {
                if (!IsReadOnlyExpenses)
                {
                    double val;
                    double.TryParse(value, out val); 
                    this.project.TotalExpenses = val;
                    OnPropertyChanged(string.Empty);
                    this.CalculateIRR();
                }
            }
        }

        public bool IsReadOnlyExpenses
        {
            get
            {
                return (this.project.ExpenseItems != null &&
                        this.project.ExpenseItems.Count > 0);
            }
        }

        public string TotalIncomes
        {
            get { return this.project.TotalIncomes.ToString("N2"); }
            set
            {
                if (!IsReadOnlyIncomes)
                {
                    double val;
                    double.TryParse(value, out val);
                    this.project.TotalIncomes = val;
                    OnPropertyChanged(string.Empty);
                    this.CalculateIRR();
                }
            }
        }

        public bool IsReadOnlyIncomes
        {
            get
            {
                return (this.project.IncomeItems != null &&
                        this.project.IncomeItems.Count > 0);
            }
        }

        public string AnalysisPeriod
        {
            get { return this.project.AnalysisPeriod.ToString(); }
            set
            {
                int val;
                int.TryParse(value, out val);
                this.project.AnalysisPeriod = Math.Abs(val);
                OnPropertyChanged(string.Empty);
                this.CalculateIRR();
            }

        }

        public string PeriodUnit
        {
            get { return this.project.PeriodUnit; }
            set { this.project.PeriodUnit = value; }
        }

        public string DiscountRate
        {
            get { return this.project.DiscountRate.ToString("P2"); }
            set
            {
                double val;
                double.TryParse(value.Replace('%',' ').Trim(), out val); 
                this.project.DiscountRate = Math.Abs(val / 100);
                OnPropertyChanged(string.Empty);
            }
        }

        private string irr;
        public string IRR
        {
            get
            {
                if (this.irr == null)
                {
                    this.irr = "wait";
                    this.CalculateIRR();
                }

                return this.irr;
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

        public string ROI
        {
            get { return this.project.CalculateROI().ToString("P2"); }
        }

        public string Payback
        {
            get { return this.project.CalculatePayback().ToString("N1"); }
        }
 
        public MainPageViewModel()
        {
            this.project = (ProjectData)App.Current.Resources["ProjectData"];
        }


        public ICommand NewProject
        {
            get
            {
                if (this.newProject == null)
                {
                    this.newProject = new DelegateCommand<object>(HandleNewProjectCommand);
                }
                return this.newProject;
            }
        }

        private void HandleNewProjectCommand(object parameter)
        {
            this.project.ProjectName = "Your Empty Project";
            this.project.MeasureUnit = "thousands EUR";
            this.project.AnalysisPeriod = 10;
            this.project.PeriodUnit = "Years";
            this.project.DiscountRate = 0.1;
            this.project.TotalInvestment = 0;
            this.project.TotalExpenses = 0;
            this.project.TotalIncomes = 0;
            this.project.InvestmentItems.Clear();
            this.project.ExpenseItems.Clear();
            this.project.IncomeItems.Clear();
            this.irr = null;
            OnPropertyChanged(string.Empty);
        }

        public ICommand OpenProject
        {
            get
            {
                if (this.openProject == null)
                {
                    this.openProject = new DelegateCommand<string>(HandleOpenProjectCommand);
                }
                return this.openProject;
            }
        }

        private async void HandleOpenProjectCommand(string data)
        {
            this.project.InvestmentItems.Clear();
            this.project.ExpenseItems.Clear();
            this.project.IncomeItems.Clear();
            this.irr = null;
            await Newtonsoft.Json.JsonConvert.PopulateObjectAsync(data, this.project, null);
            OnPropertyChanged(string.Empty);
        }

        private async void CalculateIRR()
        {
            var IRRcalc = new IRRCalculation(this.project.GetCashFlowData());
            double irrVal = await IRRcalc.ExecuteAsync();
            if (irrVal > IRRCalculation.MaximumIrrReturnValue)
            {
                irrVal = double.NaN;
            }

            this.IRR = irrVal.ToString("P2");
            this.NPV = IRRcalc.ExecuteNPV(this.project.DiscountRate).ToString("N2");
        }
    }
}
