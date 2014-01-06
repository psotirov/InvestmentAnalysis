using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Investment_Analysis.Models
{
    public class ProjectData
    {
        public string ProjectName { get; set; }
        public string MeasureUnit { get; set; }
        public double TotalInvestment { get; set; }
        public double TotalExpenses { get; set; }
        public double TotalIncomes { get; set; }
        public int AnalysisPeriod { get; set; }
        public string PeriodUnit { get; set; }
        public double DiscountRate { get; set; }
        
        public List<AnalysisItem> InvestmentItems { get; set; }
        public List<AnalysisItem> ExpenseItems { get; set; }
        public List<AnalysisItem> IncomeItems { get; set; }

        public ProjectData()
        {
            this.ProjectName = "Your Empty Project";
            this.MeasureUnit = "thousands EUR";
            this.AnalysisPeriod = 10;
            this.PeriodUnit = "Years";
            this.DiscountRate = 0.1;
            this.InvestmentItems = new List<AnalysisItem>();
            this.ExpenseItems = new List<AnalysisItem>();
            this.IncomeItems = new List<AnalysisItem>();
        }

        public double CalculateROI()
        {
            var roi = (this.TotalIncomes - this.TotalExpenses) * this.AnalysisPeriod / this.TotalInvestment;
            return roi;
        }

        public double CalculateIRR()
        {
            // TODO: Internal Rate of Return calculation
            return 0;
        }

        public double CalculateNPV()
        {
            // TODO: Net Present Value calculation
            return 0;
        }

        public double CalculatePayback()
        {
            var payback = this.TotalInvestment/(this.TotalIncomes - this.TotalExpenses);
            if (payback <= 0 || payback > this.AnalysisPeriod)
            {
                return double.PositiveInfinity;
            }

            return payback;
        }

        public void SetSerializedData(string data)
        {
        }

        public double[] GetCashFlowData()
        {
            //// this sample throws exception in standard library method - Microsoft.VisualBasic.Financial.IRR(ref cashFlowArray)
            //var result = new double[361];
            //result[0] = -68000;
            //for (int i = 1; i < result.Length; i++)
            //{
            //    result[i] = 500;
            //}

            var result = new double[this.AnalysisPeriod + 1];
            result[0] = (-1) * this.TotalInvestment;
            for (int i = 1; i < result.Length; i++)
            {
                result[i] = this.TotalIncomes - this.TotalExpenses;
            }

            return result;
        }
    }
}
