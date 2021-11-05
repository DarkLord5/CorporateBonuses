using CorporateBonuses.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateBonuses.ViewModels
{
    public class BonusViewModel
    {
        public BonusViewModel(Bonus b)
        {
            Bonus = b;
        }
        public Bonus Bonus { get; set; }
    }
}
