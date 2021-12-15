using System;

namespace CorporateBonuses.Services
{
    public static class DataCounterService
    {
        public static DateTime EnableDate(int days)
        {
            DateTime newDate = DateTime.Today;
            if (days == 0)
            {
                newDate = newDate.AddDays(0);
            }
            else if (days % 7 == 0)
            {
                newDate = newDate.AddDays(days);
                while (newDate.DayOfWeek != DayOfWeek.Monday)
                {
                    newDate = newDate.AddDays(-1);
                }
            }
            else if (days % 30 == 0)
            {
                int month = days / 30;
                newDate = newDate.AddMonths(month);
                newDate = new DateTime(newDate.Year, newDate.Month, 1);
            }
            else if (days == 365)
            {
                newDate = newDate.AddYears(1);
                newDate = new DateTime(newDate.Year, 1, 1);
            }
            
            else
            {
                newDate = newDate.AddDays(days);
            }
            return newDate;
        }
        
    }
}
