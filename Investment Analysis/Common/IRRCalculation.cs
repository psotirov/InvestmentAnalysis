// IRRCalculation.cs - Calculate the Internal rate of return for a given set of cashflows.
// Zainco Ltd
// Author: Joseph A. Nyirenda <joseph.nyirenda@gmail.com>
//             Mai Kalange<code5p@yahoo.co.uk>
// Copyright (c) 2008 Joseph A. Nyirenda, Mai Kalange, Zainco Ltd
// Adapted by Pavel Sotirov on 04-Oct-2013
//    algorythm improved, added async calculation, some minor bugs corrected
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

using System;
using System.Threading.Tasks;

namespace Investment_Analysis.Common
{
    public class IRRCalculation
    {
        public static int MaxIterations = 5000;
        public static double Tolerance = 0.000001;
        public static double GuessValue = 0.1;

        private const int MinimumRequiredCashFlowPeriods = 2;
        public const int MaximumIrrReturnValue = 50; // = 5000%

        private readonly double[] cashFlows;
        private int numberOfIterations;
        private double irrResult;

        public IRRCalculation(double[] inputCashFlows)
        {
            cashFlows = inputCashFlows;
            this.irrResult = double.NaN;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid cash flows.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is valid cash flows; otherwise, <c>false</c>.
        /// </value>
        private bool IsValidCashFlows
        {
            //Cash flows for the first period must be positive
            //There should be at least two cash flow periods         
            get
            {             
                if (cashFlows.Length < MinimumRequiredCashFlowPeriods || (cashFlows[0] > 0))
                {
                    irrResult = double.NaN; // set invalid value
                    return false;
                    //throw new ArgumentOutOfRangeException(
                    //    "Cash flow for the first period must be negative and there should be at least two cash flow periods");
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the initial guess.
        /// </summary>
        /// <value>The initial guess.</value>
        private double InitialGuess
        {
            get
            {
                double initialGuess = (cashFlows[0] == 0) ? -1 : -1 * (1 + (cashFlows[1] / cashFlows[0]));
                return initialGuess;
            }
        }

        #region ICalculator Members
        public async Task<double> ExecuteAsync()
        {
            return await Task.Run(() => Execute());
        }

        public double Execute()
        {
            if (IsValidCashFlows)
            {
                DoNewtonRaphsonCalculation(InitialGuess);
                if (double.IsNaN(irrResult) || irrResult > MaximumIrrReturnValue)
                {
                    DoNewtonRaphsonCalculation(GuessValue);
                    if (double.IsNaN(irrResult) || irrResult > MaximumIrrReturnValue)
                    {
                        irrResult = double.NaN;
                    }
                }
            }

            return irrResult;
        }

        public double ExecuteNPV(double discountRate)
        {
            double npv = SumOfIRRPolynomial(discountRate);
            return npv;
        }

        public double ExecutePayback(double discountRate)
        {
            double payback = 0;
            int pbp = 0;
            double cumulativeCashflow = 0;
            for (int i = 0; i < cashFlows.Length; i++)
            {
                var currentCashflow = cashFlows[i];
                if (discountRate > 0)
                {
                    currentCashflow /= Math.Pow((1 + discountRate), i); // discounting the cash flow
                }

                cumulativeCashflow += currentCashflow;
                if (cumulativeCashflow > 0) // we found first positive cumulate result
                {
                    payback = pbp + Math.Abs(cumulativeCashflow - currentCashflow) / currentCashflow;
                    return payback;
                }

                if (currentCashflow > 0) // negative cashflows (investment items) are not periodical cashflow and must be excluded 
                {
                    pbp++;
                }
            }

            return double.PositiveInfinity;
        }
        #endregion

        /// <summary>
        /// Does the newton rapshon calculation.
        /// </summary>
        /// <param name="estimatedReturn">The estimated return.</param>
        /// <returns></returns>
        private void DoNewtonRaphsonCalculation(double estimatedReturn)
        {  
            irrResult = estimatedReturn;
            do
            {
                numberOfIterations++;
                irrResult = irrResult - SumOfIRRPolynomial(irrResult) / IRRDerivativeSum(irrResult);
            }
            while (!HasConverged(irrResult) && IRRCalculation.MaxIterations != numberOfIterations); 
        }


        /// <summary>
        /// Sums the of IRR polynomial.
        /// </summary>
        /// <param name="estimatedReturnRate">The estimated return rate.</param>
        /// <returns></returns>
        private double SumOfIRRPolynomial(double estimatedReturnRate)
        {
            double sumOfPolynomial = 0;
            if (IsValidIterationBounds(estimatedReturnRate))
                for (int j = 0; j < cashFlows.Length; j++)
                {
                    sumOfPolynomial += cashFlows[j]/(Math.Pow((1 + estimatedReturnRate), j));
                }
            return sumOfPolynomial;
        }

        /// <summary>
        /// Determines whether the specified estimated return rate has converged.
        /// </summary>
        /// <param name="estimatedReturnRate">The estimated return rate.</param>
        /// <returns>
        /// 	<c>true</c> if the specified estimated return rate has converged; otherwise, <c>false</c>.
        /// </returns>
        private bool HasConverged(double estimatedReturnRate)
        {
            //Check that the calculated value makes the IRR polynomial zero.
            return Math.Abs(SumOfIRRPolynomial(estimatedReturnRate)) <= IRRCalculation.Tolerance;
        }

        /// <summary>
        /// IRRs the derivative sum.
        /// </summary>
        /// <param name="estimatedReturnRate">The estimated return rate.</param>
        /// <returns></returns>
        private double IRRDerivativeSum(double estimatedReturnRate)
        {
            double sumOfDerivative = 0;
            if (IsValidIterationBounds(estimatedReturnRate))
                for (int i = 1; i < cashFlows.Length; i++)
                {
                    sumOfDerivative += cashFlows[i]*(i)/Math.Pow((1 + estimatedReturnRate), i);
                }
            return sumOfDerivative*-1;
        }

        /// <summary>
        /// Determines whether [is valid iteration bounds] [the specified estimated return rate].
        /// </summary>
        /// <param name="estimatedReturnRate">The estimated return rate.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid iteration bounds] [the specified estimated return rate]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValidIterationBounds(double estimatedReturnRate)
        {
            return estimatedReturnRate != -1 && (estimatedReturnRate < int.MaxValue) &&
                   (estimatedReturnRate > int.MinValue);
        }
    }
}
