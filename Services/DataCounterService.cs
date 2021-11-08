using CorporateBonuses.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateBonuses.Services
{
    public static class DataCounterService
    {
        public static DateTime EnableDate(int expectation)
        {
            DateTime newDate = DateTime.Today;
            if (expectation == 0)
                return newDate;
            if (expectation % 30 == 0)
            {
                int month = expectation / 30;
                newDate = newDate.AddMonths(month);

                return new DateTime(newDate.Year, newDate.Month, 1);
            }
            else if (expectation == 365)
            {
                newDate = newDate.AddYears(1);
                return new DateTime(newDate.Year, 1, 1);
            }
            else if (expectation % 7 == 0)
            {
                newDate = newDate.AddDays(expectation);
                while (newDate.DayOfWeek != DayOfWeek.Monday)
                {
                    newDate = newDate.AddDays(-1);
                }
                return newDate;
            }
            else
            {
                return newDate.AddDays(expectation);
            }
        }
        
    }
}
