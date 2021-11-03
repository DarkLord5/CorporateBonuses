using CorporateBonuses.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateBonuses.ViewModels
{
    public class RequestsViewModel
    {
        public BonRequest Requests { get; set; }
        public User Users { get; set; }
        public Bonus Bonuses { get; set; }
    }
}
