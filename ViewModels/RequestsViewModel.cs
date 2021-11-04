using CorporateBonuses.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateBonuses.ViewModels
{
    public class RequestsViewModel
    {
        public List<BonRequest> Requests { get; set; }
        public List<User> Users { get; set; }
        public List<Bonus> Bonuses { get; set; }
        public SelectList States { get; set; }
        public string State { get; set; }
        public double[] Price { get; set; }
    }
}
